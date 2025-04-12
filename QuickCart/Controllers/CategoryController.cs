using Microsoft.AspNetCore.Mvc;

namespace QuickCart.Controllers
{
    public class CategoryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
