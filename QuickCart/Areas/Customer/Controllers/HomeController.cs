using System.Diagnostics;
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

    public IActionResult Index()
    {
        IEnumerable<Product> prductList = _unitOfWork.Product.GetAll(includedProperties:"Category").ToList();
        return View(prductList);
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
