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

        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            // Fetch the category from the database
            var categoryFromDb = _db.Categories.Find(id);
            if (categoryFromDb == null)
            {
                return NotFound();
            }
            return View(categoryFromDb);
        }
        [HttpPost]
        public IActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                // Update the category in the database
                _db.Categories.Update(category);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            // If model state is not valid, return the view with the current model
            return View(category);
        }
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            // Fetch the category from the database
            var categoryFromDb = _db.Categories.Find(id);
            if (categoryFromDb == null)
            {
                return NotFound();
            }
            return View(categoryFromDb);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult Delete(int id)
        {
            var categoryFromDb = _db.Categories.Find(id);
            if (categoryFromDb == null)
            {
                return NotFound();
            }
            // Remove the category from the database
            _db.Categories.Remove(categoryFromDb);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
