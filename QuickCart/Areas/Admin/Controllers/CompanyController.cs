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
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CompanyController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            // Fetch the list of Companys from the database
            List<Company> objCompanyList = _unitOfWork.Company.GetAll().ToList();
            // Pass the list to the View
            return View(objCompanyList);
        }
        public IActionResult Upsert(int? id)
        {
            // Fetch Category Name and Id            
            
            if (id == null || id == 0)
            {
                // Return the view for creating a new Company
                return View(new Company());
            }
            else
            {
                Company company = _unitOfWork.Company.Get(u => u.Id == id);
                return View(company);
            }
        }
        [HttpPost]
        public IActionResult Upsert(Company company, int? id)
        {
            if (ModelState.IsValid)
            {
                if (company.Id == 0)
                {
                    _unitOfWork.Company.Add(company);
                }
                else
                {
                    var companyFromDb = _unitOfWork.Company.Get(u => u.Id == company.Id);
                    if (companyFromDb != null)
                    {
                        // Update the existing Company in the database
                        _unitOfWork.Company.Update(company);

                    }
                }

                _unitOfWork.Save();
                TempData["success"] = "Company saved successfully!";
                return RedirectToAction("Index");
            }
            // If model state is not valid, return the view with the current model
            else
            {
                return View(company);
            }
        }


        // This method is used to get the list of Companys in JSON format for DataTables
        #region ApI Calls
        [HttpGet]
        public IActionResult GetAll()
        {
            // Fetch the list of Companys from the database
            List<Company> objCompanyList = _unitOfWork.Company.GetAll().ToList();
            // Return the list as JSON
            return Json(new { data = objCompanyList });
        }

        // This method is used to get the details of a specific Company for editing
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var companyToBeDeleted = _unitOfWork.Company.Get(u => u.Id == id);
            if (companyToBeDeleted == null)
            {
                return Json(new { sucess = false, massege = "Erorr While Deleting" });
            }

            // Remove the Company from the database
            _unitOfWork.Company.Remove(companyToBeDeleted);
            _unitOfWork.Save();

            return Json(new { sucess = true, massege = "Delete Sucsess" });
        }
        #endregion

    }
}

