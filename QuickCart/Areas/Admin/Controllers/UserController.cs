using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuickCart.DataAccess.Repository.IRepository;
using QuickCart.Models;
using QuickCart.Models.ViewModels;
using QuickCart.Utility;

namespace QuickCart.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UserController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> ManagePermission(string? id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var userFromDb = await _unitOfWork.ApplicationUsers.GetAsync(
                u => u.Id == id,
                includedProperties: "Company");

            if (userFromDb == null)
            {
                return NotFound();
            }

            var roles = await _unitOfWork.Roles.GetAllAsync();
            var companies = await _unitOfWork.Company.GetAllAsync();

            var userRoleRecord = await _unitOfWork.UserRoles.GetAsync(ur => ur.UserId == id);
            var userRole = userRoleRecord != null
                ? roles.FirstOrDefault(r => r.Id == userRoleRecord.RoleId)?.Name
                : null;

            userFromDb.Role = userRole;

            UserVM userVM = new()
            {
                ApplicationUser = userFromDb,
                Role_List = roles.Select(role => new SelectListItem
                {
                    Text = role.Name,
                    Value = role.Name
                }),
                Company_List = companies.Select(company => new SelectListItem
                {
                    Text = company.Name,
                    Value = company.Id.ToString()
                })
            };

            return View(userVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManagePermission(UserVM userVM)
        {
            var userFromDb = await _unitOfWork.ApplicationUsers.GetAsync(
                u => u.Id == userVM.ApplicationUser.Id,
                tracked: true);

            if (userFromDb == null)
            {
                return NotFound();
            }

            // Update CompanyId
            userFromDb.CompanyId = userVM.ApplicationUser.CompanyId;

            // Remove existing role
            var existingUserRole = await _unitOfWork.UserRoles.GetAsync(x => x.UserId == userVM.ApplicationUser.Id, tracked: true);
            if (existingUserRole != null)
            {
                _unitOfWork.UserRoles.Remove(existingUserRole);
            }

            // Assign new role
            var newRole = await _unitOfWork.Roles.GetAsync(x => x.Name == userVM.ApplicationUser.Role);
            if (newRole != null)
            {
                await _unitOfWork.UserRoles.AddAsync(new IdentityUserRole<string>
                {
                    UserId = userFromDb.Id,
                    RoleId = newRole.Id
                });
            }

            _unitOfWork.ApplicationUsers.Update(userFromDb);
            await _unitOfWork.SaveAsync();

            TempData["success"] = "User permissions updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        #region API Calls
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var objUserList = (await _unitOfWork.ApplicationUsers.GetAllAsync(includedProperties: "Company")).ToList();
            var userRoles = await _unitOfWork.UserRoles.GetAllAsync();
            var roles = await _unitOfWork.Roles.GetAllAsync();

            var roleDict = roles.ToDictionary(r => r.Id, r => r.Name ?? SD.Role_Customer);
            var userRoleDict = userRoles
                .Where(ur => !string.IsNullOrEmpty(ur.UserId) && !string.IsNullOrEmpty(ur.RoleId))
                .GroupBy(ur => ur.UserId)
                .ToDictionary(g => g.Key, g => g.First().RoleId);

            var sanitizedUsers = objUserList.Select(user =>
            {
                var roleName = userRoleDict.TryGetValue(user.Id, out var roleId) && roleDict.TryGetValue(roleId, out var name)
                    ? name
                    : SD.Role_Customer;

                return new
                {
                    id = user.Id,
                    name = user.Name ?? "",
                    email = user.Email ?? "",
                    phoneNumber = user.PhoneNumber ?? "",
                    company = new { name = user.Company?.Name ?? "" },
                    role = roleName,
                    lockoutEnd = user.LockoutEnd
                };
            }).ToList();

            return Json(new { data = sanitizedUsers });
        }

        [HttpPost]
        public async Task<IActionResult> LockUnlock([FromBody] string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return Json(new { success = false, message = "Invalid user ID." });
            }

            var currentAdminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentAdminId == id)
            {
                return Json(new { success = false, message = "You cannot lock your own administrator account." });
            }

            var userFromDb = await _unitOfWork.ApplicationUsers.GetAsync(u => u.Id == id, tracked: true);
            if (userFromDb == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            if (userFromDb.LockoutEnd != null && userFromDb.LockoutEnd > DateTime.Now)
            {
                userFromDb.LockoutEnd = DateTime.Now;
            }
            else
            {
                userFromDb.LockoutEnd = DateTime.Now.AddYears(1000);
            }

            await _unitOfWork.SaveAsync();
            return Json(new { success = true, message = "User status updated successfully." });
        }
        #endregion
    }
}
