using Microsoft.AspNetCore.Mvc;
using QuickCart.Data;
using QuickCart.DataAccess.Repository.IRepository;
using QuickCart.Models;

namespace QuickCart.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryRepository _categoryRepo;

        public CategoryController(ICategoryRepository categoryRepo)
        {
            _categoryRepo = categoryRepo;
        }

        public IActionResult Index()
        {
            // Fetch the list of categories from the database
            List<Category> objCategoryList = _categoryRepo.GetAll().ToList();

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
                _categoryRepo.Add(category);
                _categoryRepo.Save();

                TempData["success"] = "Category created successfully!";
                // Redirect to the Index action after successful creation
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
            var categoryFromDb = _categoryRepo.Get(u=> u.Id==id);
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
                _categoryRepo.Update(category);
                _categoryRepo.Save();

                TempData["success"] = "Category updated successfully!";
                // Redirect to the Index action after successful update
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
            var categoryFromDb = _categoryRepo.Get(u => u.Id == id);
            if (categoryFromDb == null)
            {
                return NotFound();
            }
            return View(categoryFromDb);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult Delete(int id)
        {
            var categoryFromDb = _categoryRepo.Get(u => u.Id == id);
            if (categoryFromDb == null)
            {
                return NotFound();
            }
            // Remove the category from the database
            _categoryRepo.Remove(categoryFromDb);
            _categoryRepo.Save();

            TempData["success"] = "Category deleted successfully!";
            // Redirect to the Index action after successful deletion
            return RedirectToAction("Index");
        }
    }
}
