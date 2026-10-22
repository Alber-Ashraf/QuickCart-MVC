using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickCart.DataAccess.Repository.IRepository;
using QuickCart.Models;
using QuickCart.Models.ViewModels;
using QuickCart.Utility;
using Stripe;
using Stripe.Checkout;

namespace QuickCart.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        [BindProperty]
        public OrderVM OrderVM { get; set; } = new();

        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        private string? GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        private bool IsAdminOrEmployee()
        {
            return User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee);
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Details(int orderId)
        {
            var orderHeader = await _unitOfWork.OrderHeader.GetAsync(
                u => u.OrderHeaderId == orderId,
                includedProperties: "ApplicationUser");

            if (orderHeader == null)
            {
                return NotFound();
            }

            // BOLA / IDOR defense: ensure regular customers can only view their own orders
            var currentUserId = GetCurrentUserId();
            if (!IsAdminOrEmployee() && orderHeader.ApplicationUserId != currentUserId)
            {
                return Forbid();
            }

            var orderDetails = await _unitOfWork.OrderDetails.GetAllAsync(
                u => u.OrderHeaderId == orderId,
                includedProperties: "Product");

            OrderVM = new()
            {
                OrderHeader = orderHeader,
                OrderDetail = orderDetails
            };

            return View(OrderVM);
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderDetail()
        {
            var orderHeaderFromDb = await _unitOfWork.OrderHeader.GetAsync(
                u => u.OrderHeaderId == OrderVM.OrderHeader.OrderHeaderId,
                tracked: true);

            if (orderHeaderFromDb == null)
            {
                return NotFound();
            }

            orderHeaderFromDb.Name = OrderVM.OrderHeader.Name;
            orderHeaderFromDb.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
            orderHeaderFromDb.Address = OrderVM.OrderHeader.Address;
            orderHeaderFromDb.City = OrderVM.OrderHeader.City;
            orderHeaderFromDb.State = OrderVM.OrderHeader.State;
            orderHeaderFromDb.PostalCode = OrderVM.OrderHeader.PostalCode;

            if (!string.IsNullOrEmpty(OrderVM.OrderHeader.Carrier))
            {
                orderHeaderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
            }
            if (!string.IsNullOrEmpty(OrderVM.OrderHeader.TrackingNumber))
            {
                orderHeaderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            }

            _unitOfWork.OrderHeader.Update(orderHeaderFromDb);
            await _unitOfWork.SaveAsync();

            TempData["success"] = "Order Details Updated Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = orderHeaderFromDb.OrderHeaderId });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartProcessing()
        {
            _unitOfWork.OrderHeader.UpdateStatus(OrderVM.OrderHeader.OrderHeaderId, SD.StatusInProcess, OrderVM.OrderHeader.PaymentStatus);
            await _unitOfWork.SaveAsync();

            TempData["success"] = "Order is now in process.";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.OrderHeaderId });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartShipping()
        {
            var orderHeaderFromDb = await _unitOfWork.OrderHeader.GetAsync(
                u => u.OrderHeaderId == OrderVM.OrderHeader.OrderHeaderId,
                tracked: true);

            if (orderHeaderFromDb == null)
            {
                return NotFound();
            }

            orderHeaderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            orderHeaderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
            orderHeaderFromDb.OrderStatus = SD.StatusShipped;
            orderHeaderFromDb.ShippingDate = DateTime.Now;

            if (orderHeaderFromDb.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                orderHeaderFromDb.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            }

            _unitOfWork.OrderHeader.Update(orderHeaderFromDb);
            await _unitOfWork.SaveAsync();

            TempData["success"] = "Order Shipped Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.OrderHeaderId });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder()
        {
            var orderHeaderFromDb = await _unitOfWork.OrderHeader.GetAsync(
                u => u.OrderHeaderId == OrderVM.OrderHeader.OrderHeaderId,
                tracked: true);

            if (orderHeaderFromDb == null)
            {
                return NotFound();
            }

            if (orderHeaderFromDb.PaymentStatus == SD.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeaderFromDb.PaymentIntentId
                };

                var service = new RefundService();
                await service.CreateAsync(options);

                _unitOfWork.OrderHeader.UpdateStatus(orderHeaderFromDb.OrderHeaderId, SD.StatusCancelled, SD.StatusRefunded);
            }
            else
            {
                _unitOfWork.OrderHeader.UpdateStatus(orderHeaderFromDb.OrderHeaderId, SD.StatusCancelled, SD.StatusCancelled);
            }

            await _unitOfWork.SaveAsync();
            TempData["success"] = "Order Cancelled Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.OrderHeaderId });
        }

        [HttpPost]
        [ActionName("Details")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details_PAY_NOW()
        {
            var orderHeader = await _unitOfWork.OrderHeader.GetAsync(
                u => u.OrderHeaderId == OrderVM.OrderHeader.OrderHeaderId,
                includedProperties: "ApplicationUser");

            if (orderHeader == null)
            {
                return NotFound();
            }

            var orderDetails = await _unitOfWork.OrderDetails.GetAllAsync(
                u => u.OrderHeaderId == OrderVM.OrderHeader.OrderHeaderId,
                includedProperties: "Product");

            OrderVM.OrderHeader = orderHeader;
            OrderVM.OrderDetail = orderDetails;

            var domain = $"{Request.Scheme}://{Request.Host}/";
            var options = new SessionCreateOptions
            {
                SuccessUrl = domain + $"Admin/Order/PaymentConfirmation?orderHeaderId={OrderVM.OrderHeader.OrderHeaderId}",
                CancelUrl = domain + $"Admin/Order/Details?orderId={OrderVM.OrderHeader.OrderHeaderId}",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };

            foreach (var item in OrderVM.OrderDetail)
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

            _unitOfWork.OrderHeader.UpdateStripePaymentId(OrderVM.OrderHeader.OrderHeaderId, session.Id, session.PaymentIntentId);
            await _unitOfWork.SaveAsync();

            Response.Headers.Append("Location", session.Url);
            return new StatusCodeResult(303);
        }

        public async Task<IActionResult> PaymentConfirmation(int orderHeaderId)
        {
            var orderHeader = await _unitOfWork.OrderHeader.GetAsync(u => u.OrderHeaderId == orderHeaderId, tracked: true);
            if (orderHeader != null && orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment && !string.IsNullOrEmpty(orderHeader.SessionId))
            {
                var service = new SessionService();
                Session session = await service.GetAsync(orderHeader.SessionId);

                if (session.PaymentStatus.Equals("paid", StringComparison.OrdinalIgnoreCase))
                {
                    _unitOfWork.OrderHeader.UpdateStripePaymentId(orderHeaderId, session.Id, session.PaymentIntentId);
                    _unitOfWork.OrderHeader.UpdateStatus(orderHeaderId, orderHeader.OrderStatus ?? SD.StatusApproved, SD.PaymentStatusApproved);
                    await _unitOfWork.SaveAsync();
                }
            }

            return View(orderHeaderId);
        }

        #region API CALLS
        [HttpGet]
        public async Task<IActionResult> GetAll(string? status)
        {
            IEnumerable<OrderHeader> objOrderHeaders;

            if (IsAdminOrEmployee())
            {
                objOrderHeaders = await _unitOfWork.OrderHeader.GetAllAsync(includedProperties: "ApplicationUser");
            }
            else
            {
                var userId = GetCurrentUserId();
                objOrderHeaders = await _unitOfWork.OrderHeader.GetAllAsync(
                    u => u.ApplicationUserId == userId,
                    includedProperties: "ApplicationUser");
            }

            switch (status?.ToLower())
            {
                case "pending":
                    objOrderHeaders = objOrderHeaders.Where(u => u.PaymentStatus == SD.PaymentStatusDelayedPayment);
                    break;
                case "inprocess":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusInProcess);
                    break;
                case "completed":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusShipped);
                    break;
                case "approved":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusApproved);
                    break;
                default:
                    break;
            }

            return Json(new { data = objOrderHeaders });
        }
        #endregion
    }
}
