using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuickCart.DataAccess.Repository.IRepository;
using QuickCart.Models;
using QuickCart.Models.ViewModels;
using QuickCart.Utility;

namespace QuickCart.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
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
        public IActionResult Upsert(ProductVM productVM, List<IFormFile>? files, int? id)
        {
            // Check if the file is not null and has a valid size
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (files != null)
                {
                    // Loop through each file in the list
                    foreach (IFormFile file in files) 
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        string productPath = @"images\products\product-" + productVM.Product.Id;
                        string finalPath = Path.Combine(wwwRootPath, productPath);

                        // Check if the directory exists, if not create it
                        if (!Directory.Exists(finalPath))
                            Directory.CreateDirectory(finalPath);

                        using (var fileStream = new FileStream(Path.Combine(finalPath, fileName), FileMode.Create))
                        {
                            file.CopyTo(fileStream);
                        }

                        // Create a new ProductImage object
                        ProductImage productImage = new()
                        {
                            ImageUrl = @"\" + productPath + @"\" + fileName,
                            ProductId = productVM.Product.Id,
                        };

                        // Check if the Product already has images
                        if (productVM.Product.ProductImages == null)
                            productVM.Product.ProductImages = new List<ProductImage>();

                        productVM.Product.ProductImages.Add(productImage);
                    }

                    _unitOfWork.Product.Update(productVM.Product);
                    _unitOfWork.Save();
                }

                TempData["success"] = "Product created/updated successfully";

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
        
        // This method is used to get the list of Products in JSON format for DataTables
        #region ApI Calls
        [HttpGet]
        public IActionResult GetAll()
        {
            // Fetch the list of Products from the database
            List<Product> objProductList = _unitOfWork.Product.GetAll(includedProperties: "Category").ToList();
            // Return the list as JSON
            return Json(new { data = objProductList });
        }

        // This method is used to get the details of a specific Product for editing
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var productToBeDeleted = _unitOfWork.Product.Get(u => u.Id == id);
            if (productToBeDeleted == null)
            {
                return Json(new { sucess = false, massege = "Erorr While Deleting" });
            }

            //var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, productToBeDeleted.ImageURL.TrimStart('\\'));
            //if (System.IO.File.Exists(oldImagePath))
            //{
            //    System.IO.File.Delete(oldImagePath);
            //}
            // Remove the Product from the database
            _unitOfWork.Product.Remove(productToBeDeleted);
            _unitOfWork.Save();

            return Json(new { sucess = true, massege = "Delete Sucsess" });
        }
        #endregion

    }
}

