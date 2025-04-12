using Microsoft.AspNetCore.Mvc;
using QuickCart.Data;
using QuickCart.Models;

namespace QuickCart.Controllers
{
    public class CategoryController : Controller
    {
        private readonly QuickCartDbContext _db;

        public CategoryController(QuickCartDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            // Fetch the list of categories from the database
            List<Category> objCategoryList = _db.Categories.ToList();

            // Pass the list to the View
            return View(objCategoryList);
        }
    }
}
