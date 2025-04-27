using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuickCart.DataAccess.Repository.IRepository;
using QuickCart.Models;
using QuickCart.Models.ViewModels;

namespace QuickCart.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            // Fetch the list of Products from the database
            List<Product> objProductList = _unitOfWork.Product.GetAll().ToList();
            // Pass the list to the View
            return View(objProductList);
        }
        public IActionResult Create()
        {
            // Fetch Category Name and Id            
            ProductVM productVM = new()
            {
                Product = new Product(),
                CategoryList = _unitOfWork.Category.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                })
            };

            // Return the view for creating a new Product
            return View(productVM);
        }
        [HttpPost]
        public IActionResult Create(ProductVM productVM)
        {
            if (ModelState.IsValid)
            {
                // Add the new Product to the database
                _unitOfWork.Product.Add(productVM.Product);
                _unitOfWork.Save();
                TempData["success"] = "Product created successfully!";
                // Redirect to the Index action after successful creation
                return RedirectToAction("Index");
            }
            // If model state is not valid, return the view with the current model
            else
            {
                productVM.CategoryList = _unitOfWork.Category.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                });
                return View(productVM);
            }
        }

        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            // Fetch the Product from the database
            var productFromDb = _unitOfWork.Product.Get(u => u.Id == id);
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
                _unitOfWork.Product.Update(product);
                _unitOfWork.Save();

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
            var productFromDb = _unitOfWork.Product.Get(u => u.Id == id);
            if (productFromDb == null)
            {
                return NotFound();
            }
            return View(productFromDb);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult Delete(int id)
        {
            var productFromDb = _unitOfWork.Product.Get(u => u.Id == id);
            if (productFromDb == null)
            {
                return NotFound();
            }
            // Remove the Product from the database
            _unitOfWork.Product.Remove(productFromDb);
            _unitOfWork.Save();

            TempData["success"] = "Product deleted successfully!";
            // Redirect to the Index action after successful deletion
            return RedirectToAction("Index");
        }
    }
}
