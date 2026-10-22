using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuickCart.DataAccess.Repository.IRepository;
using QuickCart.Models;
using QuickCart.Models.ViewModels;
using QuickCart.Utility;
using Stripe.Checkout;

namespace QuickCart.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; } = new();

        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        private string? GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var cartList = (await _unitOfWork.ShoppingCart.GetAllAsync(
                x => x.ApplicationUserId == userId,
                includedProperties: "Product")).ToList();

            var productIds = cartList.Select(x => x.ProductId).Distinct().ToList();
            var productImages = productIds.Any()
                ? (await _unitOfWork.ProductImage.GetAllAsync(pi => productIds.Contains(pi.ProductId))).ToList()
                : new List<ProductImage>();

            ShoppingCartVM = new()
            {
                ShoppingCartList = cartList,
                OrderHeader = new()
            };

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Product.ProductImages = productImages.Where(x => x.ProductId == cart.ProductId).ToList();
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            return View(ShoppingCartVM);
        }

        public async Task<IActionResult> Plus(int cartId)
        {
            var userId = GetCurrentUserId();
            var cart = await _unitOfWork.ShoppingCart.GetAsync(x => x.Id == cartId, tracked: true);

            // BOLA / IDOR Defense: verify item belongs to currently authenticated user
            if (cart == null || cart.ApplicationUserId != userId)
            {
                return Forbid();
            }

            if (cart.Count < 1000)
            {
                cart.Count += 1;
                _unitOfWork.ShoppingCart.Update(cart);
                await _unitOfWork.SaveAsync();
            }
            else
            {
                TempData["warning"] = "Maximum quantity per item is 1000.";
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Minus(int cartId)
        {
            var userId = GetCurrentUserId();
            var cart = await _unitOfWork.ShoppingCart.GetAsync(x => x.Id == cartId, tracked: true);

            // BOLA / IDOR Defense: verify item belongs to currently authenticated user
            if (cart == null || cart.ApplicationUserId != userId)
            {
                return Forbid();
            }

            if (cart.Count <= 1)
            {
                _unitOfWork.ShoppingCart.Remove(cart);
                await _unitOfWork.SaveAsync();

                var remainingCount = (await _unitOfWork.ShoppingCart.GetAllAsync(u => u.ApplicationUserId == userId, tracking: false)).Count();
                HttpContext.Session.SetInt32(SD.SessionCart, remainingCount);
            }
            else
            {
                cart.Count -= 1;
                _unitOfWork.ShoppingCart.Update(cart);
                await _unitOfWork.SaveAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Remove(int cartId)
        {
            var userId = GetCurrentUserId();
            var cart = await _unitOfWork.ShoppingCart.GetAsync(x => x.Id == cartId, tracked: true);

            // BOLA / IDOR Defense: verify item belongs to currently authenticated user
            if (cart == null || cart.ApplicationUserId != userId)
            {
                return Forbid();
            }

            _unitOfWork.ShoppingCart.Remove(cart);
            await _unitOfWork.SaveAsync();

            var remainingCount = (await _unitOfWork.ShoppingCart.GetAllAsync(u => u.ApplicationUserId == userId, tracking: false)).Count();
            HttpContext.Session.SetInt32(SD.SessionCart, remainingCount);

            TempData["success"] = "Item removed from cart.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Summary()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var cartItems = (await _unitOfWork.ShoppingCart.GetAllAsync(
                x => x.ApplicationUserId == userId,
                includedProperties: "Product")).ToList();

            if (!cartItems.Any())
            {
                TempData["error"] = "Your cart is empty. Please add items before checking out.";
                return RedirectToAction(nameof(Index));
            }

            var applicationUser = await _unitOfWork.ApplicationUsers.GetAsync(x => x.Id == userId);

            ShoppingCartVM = new()
            {
                ShoppingCartList = cartItems,
                OrderHeader = new()
                {
                    ApplicationUser = applicationUser,
                    Name = applicationUser?.Name ?? "",
                    PhoneNumber = applicationUser?.PhoneNumber ?? "",
                    Address = applicationUser?.Address ?? "",
                    City = applicationUser?.City ?? "",
                    State = applicationUser?.State ?? "",
                    PostalCode = applicationUser?.PostalCode ?? ""
                }
            };

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            return View(ShoppingCartVM);
        }

        [HttpPost]
        [ActionName("Summary")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SummaryPost()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var cartItems = (await _unitOfWork.ShoppingCart.GetAllAsync(
                x => x.ApplicationUserId == userId,
                includedProperties: "Product")).ToList();

            if (!cartItems.Any())
            {
                TempData["error"] = "Cannot checkout with an empty cart.";
                return RedirectToAction(nameof(Index));
            }

            ShoppingCartVM.ShoppingCartList = cartItems;
            ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = userId;
            ShoppingCartVM.OrderHeader.OrderTotal = 0;

            var applicationUser = await _unitOfWork.ApplicationUsers.GetAsync(x => x.Id == userId);

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            if (applicationUser != null && applicationUser.CompanyId.GetValueOrDefault() > 0)
            {
                // Approved business company account with 30-day payment term
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
            }
            else
            {
                // Regular customer: capture payment via Stripe
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            }

            await _unitOfWork.OrderHeader.AddAsync(ShoppingCartVM.OrderHeader);
            await _unitOfWork.SaveAsync();

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderHeaderId = ShoppingCartVM.OrderHeader.OrderHeaderId,
                    Count = cart.Count,
                    Price = cart.Price
                };
                await _unitOfWork.OrderDetails.AddAsync(orderDetail);
            }
            await _unitOfWork.SaveAsync();

            if (applicationUser != null && applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                // Stripe Checkout Session Creation
                var domain = $"{Request.Scheme}://{Request.Host}/";

                var options = new SessionCreateOptions
                {
                    SuccessUrl = domain + $"Customer/Cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.OrderHeaderId}",
                    CancelUrl = domain + "Customer/Cart/Index",
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                };

                foreach (var item in ShoppingCartVM.ShoppingCartList)
                {
                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = Convert.ToInt64(Math.Round(item.Price * 100)),
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.Title
                            }
                        },
                        Quantity = item.Count
                    };
                    options.LineItems.Add(sessionLineItem);
                }

                var service = new SessionService();
                Session session = await service.CreateAsync(options);

                _unitOfWork.OrderHeader.UpdateStripePaymentId(
                    ShoppingCartVM.OrderHeader.OrderHeaderId,
                    session.Id,
                    session.PaymentIntentId);
                await _unitOfWork.SaveAsync();

                Response.Headers.Append("Location", session.Url);
                return new StatusCodeResult(303);
            }

            return RedirectToAction(nameof(OrderConfirmation), new { id = ShoppingCartVM.OrderHeader.OrderHeaderId });
        }

        public async Task<IActionResult> OrderConfirmation(int id)
        {
            var userId = GetCurrentUserId();
            var orderHeader = await _unitOfWork.OrderHeader.GetAsync(
                u => u.OrderHeaderId == id,
                includedProperties: "ApplicationUser");

            // BOLA / IDOR Defense: verify order ownership unless admin/employee
            if (orderHeader == null)
            {
                return NotFound();
            }

            if (!User.IsInRole(SD.Role_Admin) && !User.IsInRole(SD.Role_Employee) && orderHeader.ApplicationUserId != userId)
            {
                return Forbid();
            }

            if (orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment && !string.IsNullOrEmpty(orderHeader.SessionId))
            {
                var service = new SessionService();
                Session session = await service.GetAsync(orderHeader.SessionId);

                if (session.PaymentStatus.Equals("paid", StringComparison.OrdinalIgnoreCase))
                {
                    _unitOfWork.OrderHeader.UpdateStripePaymentId(id, session.Id, session.PaymentIntentId);
                    _unitOfWork.OrderHeader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                    await _unitOfWork.SaveAsync();
                }
            }

            var shoppingCarts = (await _unitOfWork.ShoppingCart.GetAllAsync(u => u.ApplicationUserId == orderHeader.ApplicationUserId)).ToList();
            _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
            await _unitOfWork.SaveAsync();

            HttpContext.Session.SetInt32(SD.SessionCart, 0);

            return View(id);
        }

        private static double GetPriceBasedOnQuantity(ShoppingCart shoppingCart)
        {
            if (shoppingCart.Count <= 50)
            {
                return shoppingCart.Product.Price;
            }
            if (shoppingCart.Count <= 100)
            {
                return shoppingCart.Product.Price50;
            }
            return shoppingCart.Product.Price100;
        }
    }
}
