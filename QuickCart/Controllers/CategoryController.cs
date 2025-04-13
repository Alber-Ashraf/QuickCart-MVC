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
        public IActionResult Create()
        {
            // Return the view for creating a new category
            return View();
        }
        [HttpPost]
        public IActionResult Create(Category category)
        {
            if (ModelState.IsValid)
            {
                // Add the new category to the database
                _db.Categories.Add(category);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            // If model state is not valid, return the view with the current model
            return View(category);
        }
    }
}
