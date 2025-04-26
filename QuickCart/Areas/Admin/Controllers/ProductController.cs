using Microsoft.AspNetCore.Mvc;
using QuickCart.DataAccess.Repository.IRepository;
using QuickCart.Models;

namespace QuickCart.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepo;

        public ProductController(IProductRepository productRepo)
        {
            _productRepo = productRepo;
        }

        public IActionResult Index()
        {
            // Fetch the list of Products from the database
            List<Product> objProductList = _productRepo.GetAll().ToList();

            // Pass the list to the View
            return View(objProductList);
        }
        public IActionResult Create()
        {
            // Return the view for creating a new Product
            return View();
        }
        [HttpPost]
        public IActionResult Create(Product product)
        {
            if (ModelState.IsValid)
            {
                // Add the new Product to the database
                _productRepo.Add(product);
                _productRepo.Save();

                TempData["success"] = "Product created successfully!";
                // Redirect to the Index action after successful creation
                return RedirectToAction("Index");
            }
            // If model state is not valid, return the view with the current model
            return View(product);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            // Fetch the Product from the database
            var productFromDb = _productRepo.Get(u => u.Id == id);
            if (productFromDb == null)
            {
                return NotFound();
            }
            return View(productFromDb);
        }
        [HttpPost]
        public IActionResult Edit(Product product)
        {
            if (ModelState.IsValid)
            {
                // Update the Product in the database
                _productRepo.Update(product);
                _productRepo.Save();

                TempData["success"] = "Product updated successfully!";
                // Redirect to the Index action after successful update
                return RedirectToAction("Index");
            }
            // If model state is not valid, return the view with the current model
            return View(product);
        }
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            // Fetch the Product from the database
            var productFromDb = _productRepo.Get(u => u.Id == id);
            if (productFromDb == null)
            {
                return NotFound();
            }
            return View(productFromDb);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult Delete(int id)
        {
            var productFromDb = _productRepo.Get(u => u.Id == id);
            if (productFromDb == null)
            {
                return NotFound();
            }
            // Remove the Product from the database
            _productRepo.Remove(productFromDb);
            _productRepo.Save();

            TempData["success"] = "Product deleted successfully!";
            // Redirect to the Index action after successful deletion
            return RedirectToAction("Index");
        }
    }
}
