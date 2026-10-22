using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickCart.DataAccess.Repository.IRepository;
using QuickCart.Models;
using QuickCart.Models.ViewModels;
using QuickCart.Utility;

namespace QuickCart.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index(string? search, int? categoryId)
        {
            var categories = await _unitOfWork.Category.GetAllAsync();
            ViewBag.Categories = categories.OrderBy(c => c.DisplayOrder).ToList();
            ViewBag.SelectedCategory = categoryId;
            ViewBag.SearchQuery = search;

            var products = await _unitOfWork.Product.GetAllAsync(includedProperties: "Category,ProductImages");

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                products = products.Where(p => p.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                products = products.Where(p =>
                    p.Title.ToLower().Contains(term) ||
                    p.Author.ToLower().Contains(term) ||
                    (p.ISBN != null && p.ISBN.ToLower().Contains(term)));
            }

            return View(products.ToList());
        }

        public async Task<IActionResult> Details(int productId)
        {
            if (productId <= 0)
            {
                return NotFound();
            }

            var product = await _unitOfWork.Product.GetAsync(
                u => u.Id == productId,
                includedProperties: "Category,ProductImages");

            if (product == null)
            {
                return NotFound();
            }

            ShoppingCart cartObj = new()
            {
                Product = product,
                Count = 1,
                ProductId = productId
            };

            return View(cartObj);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(ShoppingCart shoppingCart)
        {
            if (shoppingCart.Count < 1 || shoppingCart.Count > 1000)
            {
                TempData["error"] = "Please select a valid quantity between 1 and 1000.";
                return RedirectToAction(nameof(Details), new { productId = shoppingCart.ProductId });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            shoppingCart.ApplicationUserId = userId;

            var cartFromDb = await _unitOfWork.ShoppingCart.GetAsync(
                u => u.ApplicationUserId == userId && u.ProductId == shoppingCart.ProductId,
                tracked: true);

            if (cartFromDb != null)
            {
                cartFromDb.Count += shoppingCart.Count;
                _unitOfWork.ShoppingCart.Update(cartFromDb);
                await _unitOfWork.SaveAsync();
            }
            else
            {
                await _unitOfWork.ShoppingCart.AddAsync(shoppingCart);
                await _unitOfWork.SaveAsync();

                var userCartItems = await _unitOfWork.ShoppingCart.GetAllAsync(u => u.ApplicationUserId == userId);
                HttpContext.Session.SetInt32(SD.SessionCart, userCartItems.Count());
            }

            TempData["success"] = "Item added to cart successfully!";
            return RedirectToAction(nameof(Index));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
