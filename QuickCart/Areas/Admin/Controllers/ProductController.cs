using Microsoft.AspNetCore.Hosting;
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
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            // Fetch the list of Products from the database
            List<Product> objProductList = _unitOfWork.Product.GetAll(includedProperties:"Category").ToList();
            // Pass the list to the View
            return View(objProductList);
        }
        public IActionResult Upsert(int? id)
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
            if (id == null || id == 0)
            {
                // Return the view for creating a new Product
                return View(productVM);
            }
            else
            {
                productVM.Product = _unitOfWork.Product.Get(u => u.Id == id);
                return View(productVM);
            }
        }
        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, IFormFile? file, int? id)
        {
            // Check if the file is not null and has a valid size
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;


                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"images\products");

                    if (!string.IsNullOrEmpty(productVM.Product.ImageURL))
                    {
                        // If the product already has an image, delete the old image
                        var oldImagePath = Path.Combine(wwwRootPath, productVM.Product.ImageURL.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    productVM.Product.ImageURL = @"\images\products\" + fileName;
                }

                if (id == null || id == 0)
                {
                    // Add the new Product to the database
                    _unitOfWork.Product.Add(productVM.Product);
                    _unitOfWork.Save();
                    TempData["success"] = "Product created successfully!";
                }
                else
                {
                    // Update the existing Product in the database
                    _unitOfWork.Product.Update(productVM.Product);
                    _unitOfWork.Save();
                    TempData["success"] = "Product updated successfully!";
                }
                    // Redirect to the Index action after successful update
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
