using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickCart.DataAccess.Repository.IRepository;
using QuickCart.Models;

namespace QuickCart.Areas.Customer.Controllers;

[Area("Customer")]
public class HomeController : Controller
{
    // Injecting the logger service to log messages
    private readonly ILogger<HomeController> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    // Fetching the list of products from the database
    public IActionResult Index()
    {
        IEnumerable<Product> prductList = _unitOfWork.Product.GetAll(includedProperties:"Category").ToList();
        return View(prductList);
    }

    // Fetching the details of a specific product
    public IActionResult Details(int productId)
    {
        ShoppingCart cartObj = new()
        {
            Product = _unitOfWork.Product.Get(u => u.Id == productId, includedProperties: "Category"),
            Count = 1,
            ProductId = productId
        };
        
        return View(cartObj);
    }

    [HttpPost]
    [Authorize]
    public IActionResult Details(ShoppingCart shoppingCart)
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

        shoppingCart.ApplicationUserId = userId;

        ShoppingCart cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.ApplicationUserId == userId && u.ProductId == shoppingCart.ProductId);

        if (cartFromDb != null)
        {
            cartFromDb.Count += shoppingCart.Count;
            _unitOfWork.ShoppingCart.Update(cartFromDb);
        }
        else
        {
            _unitOfWork.ShoppingCart.Add(shoppingCart);
        }

        _unitOfWork.Save();

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
