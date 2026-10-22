using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickCart.DataAccess.Repository.IRepository;
using QuickCart.Models;
using QuickCart.Utility;

namespace QuickCart.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            var companyList = await _unitOfWork.Company.GetAllAsync();
            return View(companyList);
        }

        public async Task<IActionResult> Upsert(int? id)
        {
            if (id == null || id == 0)
            {
                return View(new Company());
            }

            var company = await _unitOfWork.Company.GetAsync(u => u.Id == id);
            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(Company company)
        {
            if (ModelState.IsValid)
            {
                if (company.Id == 0)
                {
                    await _unitOfWork.Company.AddAsync(company);
                    TempData["success"] = "Company created successfully!";
                }
                else
                {
                    _unitOfWork.Company.Update(company);
                    TempData["success"] = "Company updated successfully!";
                }

                await _unitOfWork.SaveAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(company);
        }

        #region API Calls
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var companyList = await _unitOfWork.Company.GetAllAsync();
            return Json(new { data = companyList });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return Json(new { success = false, message = "Invalid company identifier." });
            }

            var company = await _unitOfWork.Company.GetAsync(u => u.Id == id, tracked: true);
            if (company == null)
            {
                return Json(new { success = false, message = "Company not found." });
            }

            _unitOfWork.Company.Remove(company);
            await _unitOfWork.SaveAsync();

            return Json(new { success = true, message = "Company deleted successfully." });
        }
        #endregion
    }
}
