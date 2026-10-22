using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
        private readonly IBlobService _blobService;

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment, IBlobService blobService)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
            _blobService = blobService;
        }

        public async Task<IActionResult> Index()
        {
            var productList = await _unitOfWork.Product.GetAllAsync(includedProperties: "Category");
            return View(productList);
        }

        public async Task<IActionResult> Upsert(int? id)
        {
            var categories = await _unitOfWork.Category.GetAllAsync();
            ProductVM productVM = new()
            {
                Product = new Product(),
                CategoryList = categories.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                })
            };

            if (id == null || id == 0)
            {
                return View(productVM);
            }

            var product = await _unitOfWork.Product.GetAsync(u => u.Id == id, includedProperties: "ProductImages");
            if (product == null)
            {
                return NotFound();
            }

            productVM.Product = product;
            return View(productVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(ProductVM productVM, List<IFormFile>? files)
        {
            if (ModelState.IsValid)
            {
                if (productVM.Product.Id == 0)
                {
                    await _unitOfWork.Product.AddAsync(productVM.Product);
                }
                else
                {
                    _unitOfWork.Product.Update(productVM.Product);
                }
                await _unitOfWork.SaveAsync();

                if (files != null && files.Count > 0)
                {
                    foreach (var file in files)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        var imageUrl = await _blobService.UploadFileAsync(file, fileName);

                        ProductImage image = new()
                        {
                            ImageUrl = imageUrl,
                            ProductId = productVM.Product.Id
                        };

                        await _unitOfWork.ProductImage.AddAsync(image);
                    }
                    await _unitOfWork.SaveAsync();
                }

                TempData["success"] = "Product saved successfully!";
                return RedirectToAction(nameof(Index));
            }

            var categories = await _unitOfWork.Category.GetAllAsync();
            productVM.CategoryList = categories.Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            });

            return View(productVM);
        }

        public async Task<IActionResult> DeleteImage(int? imageId)
        {
            if (imageId == null || imageId == 0)
            {
                return NotFound();
            }

            var imageToBeDeleted = await _unitOfWork.ProductImage.GetAsync(u => u.Id == imageId, tracked: true);
            if (imageToBeDeleted == null)
            {
                return NotFound();
            }

            var productId = imageToBeDeleted.ProductId;

            if (!string.IsNullOrEmpty(imageToBeDeleted.ImageUrl))
            {
                var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, imageToBeDeleted.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }

                var blobName = Path.GetFileName(imageToBeDeleted.ImageUrl);
                await _blobService.DeleteFileFromBlob(blobName);
            }

            _unitOfWork.ProductImage.Remove(imageToBeDeleted);
            await _unitOfWork.SaveAsync();

            TempData["success"] = "Image deleted successfully!";
            return RedirectToAction(nameof(Upsert), new { id = productId });
        }

        #region API Calls
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var productList = await _unitOfWork.Product.GetAllAsync(includedProperties: "Category");
            return Json(new { data = productList });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return Json(new { success = false, message = "Invalid product identifier." });
            }

            var productToBeDeleted = await _unitOfWork.Product.GetAsync(u => u.Id == id, tracked: true);
            if (productToBeDeleted == null)
            {
                return Json(new { success = false, message = "Product not found." });
            }

            _unitOfWork.Product.Remove(productToBeDeleted);
            await _unitOfWork.SaveAsync();

            return Json(new { success = true, message = "Product deleted successfully." });
        }
        #endregion
    }
}
