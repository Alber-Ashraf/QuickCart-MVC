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
                productVM.Product = _unitOfWork.Product.Get(u => u.Id == id, includedProperties: "ProductImages");
                return View(productVM);
            }
        }
        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, List<IFormFile>? files)
        {
            if (ModelState.IsValid)
            {
                // Check if it's Create or Update
                if (productVM.Product.Id == 0)
                {
                    // Create Product first to generate ID
                    _unitOfWork.Product.Add(productVM.Product);
                    _unitOfWork.Save();
                }
                else
                {
                    // Update existing product fields (excluding images here)
                    _unitOfWork.Product.Update(productVM.Product);
                    _unitOfWork.Save();
                }

                // Handle image upload
                if (files != null && files.Count > 0)
                {
                    string wwwRootPath = _webHostEnvironment.WebRootPath;
                    string productPath = Path.Combine("images", "products", "product-" + productVM.Product.Id);
                    string finalPath = Path.Combine(wwwRootPath, productPath);

                    if (!Directory.Exists(finalPath))
                        Directory.CreateDirectory(finalPath);

                    foreach (IFormFile file in files)
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        string filePath = Path.Combine(finalPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            file.CopyTo(stream);
                        }

                        // Save relative path to DB
                        ProductImage image = new ProductImage
                        {
                            ImageUrl = "/" + Path.Combine(productPath, fileName).Replace("\\", "/"),
                            ProductId = productVM.Product.Id
                        };

                        _unitOfWork.ProductImage.Add(image); // تأكد إن عندك access لـ ProductImage repo
                    }

                    _unitOfWork.Save();
                }

                TempData["success"] = "Product created/updated successfully";
                return RedirectToAction("Index");
            }

            // If not valid, return view with categories loaded
            productVM.CategoryList = _unitOfWork.Category.GetAll().Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            });

            return View(productVM);
        }

        public IActionResult DeleteImage(int? imageId)
        {
            var imageToBeDeleted = _unitOfWork.ProductImage.Get(u => u.Id == imageId);
            int productId = imageToBeDeleted.ProductId;

            if (imageToBeDeleted != null)
            {
                if (!string.IsNullOrEmpty(imageToBeDeleted.ImageUrl))
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, imageToBeDeleted.ImageUrl.TrimStart('\\'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }
                // Remove the image from the database
                _unitOfWork.ProductImage.Remove(imageToBeDeleted);
                _unitOfWork.Save();

                TempData["success"] = "Image deleted successfully";
            }
                return RedirectToAction(nameof(Upsert), new { id = productId });
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

