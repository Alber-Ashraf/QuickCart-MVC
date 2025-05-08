using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuickCart.Data;
using QuickCart.DataAccess.Repository;
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
        private readonly QuickCartDbContext _db;

        public UserController(QuickCartDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            // Pass the list to the View
            return View();
        }

        public IActionResult ManagePermission(string? id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var userFromDb = _db.ApplicationUsers
                .Include(u => u.Company)
                .FirstOrDefault(u => u.Id == id);

            if (userFromDb == null)
            {
                return NotFound();
            }

            // Get role name directly from joins
            var userRole = (from userRoles in _db.UserRoles
                            join roles in _db.Roles on userRoles.RoleId equals roles.Id
                            where userRoles.UserId == id
                            select roles.Name).FirstOrDefault();

            userFromDb.Role = userRole;

            UserVM userVM = new()
            {
                ApplicationUser = userFromDb,

                Role_List = _db.Roles.Select(role => new SelectListItem
                {
                    Text = role.Name,
                    Value = role.Name
                }),

                Company_List = _db.Companies.Select(company => new SelectListItem
                {
                    Text = company.Name,
                    Value = company.Id.ToString()
                })
            };

            return View(userVM);
        }

        [HttpPost]
        public IActionResult ManagePermission(UserVM userVM)
        {
            var userFromDb = _db.ApplicationUsers.FirstOrDefault(u => u.Id == userVM.ApplicationUser.Id);
            if (userFromDb == null)
            {
                return NotFound();
            }

            // 1. Update CompanyId
            userFromDb.CompanyId = userVM.ApplicationUser.CompanyId;

            // 2. Remove existing role
            var userRole = _db.UserRoles.FirstOrDefault(x => x.UserId == userVM.ApplicationUser.Id);
            if (userRole != null)
            {
                _db.UserRoles.Remove(userRole);
            }

            // 3. Assign new role
            var newRole = _db.Roles.FirstOrDefault(x => x.Name == userVM.ApplicationUser.Role);
            if (newRole != null)
            {
                _db.UserRoles.Add(new IdentityUserRole<string>
                {
                    UserId = userFromDb.Id,
                    RoleId = newRole.Id
                });
            }

            _db.SaveChanges();
            // 4. Update the user in the database
            _db.ApplicationUsers.Update(userFromDb);

            // 4. Show success message

            TempData["success"] = "User permissions updated successfully.";
            return RedirectToAction("Index");
        }



        // This method is used to get the list of Companys in JSON format for DataTables
        #region ApI Calls
        [HttpGet]
        public IActionResult GetAll()
        {
            // Fetch the list of Companys from the database
            List<ApplicationUser> objUserList = _db.ApplicationUsers.Include(c=>c.Company).ToList();

            var userRole = _db.UserRoles.ToList();
            var role = _db.Roles.ToList();

            foreach (var user in objUserList)
            {
                // Get the role of the user
                var roleId = userRole.FirstOrDefault(x => x.UserId == user.Id).RoleId;
                user.Role = role.FirstOrDefault(x => x.Id == roleId).Name;

                if (user.Company == null)
                {
                    // If the Company is not null, set the Company name
                    user.Company = new Company
                    {
                        Name = ""
                    };
                }

            }

            // Return the list as JSON
            return Json(new { data = objUserList });
        }

        // This method is used to get the details of a specific Company for editing
        [HttpPost]
        public IActionResult LockUnlock([FromBody] string id)
        {
            var userFromDb = _db.ApplicationUsers.FirstOrDefault(u => u.Id == id);
            if (userFromDb == null)
            {
                return Json(new { success = false, message = "Error while locking/unlocking the user." });
            }

            if (userFromDb.LockoutEnd != null && userFromDb.LockoutEnd > DateTime.Now)
            {
                userFromDb.LockoutEnd = DateTime.Now;
            }
            else
            {
                userFromDb.LockoutEnd = DateTime.Now.AddYears(1000);
            }

            _db.SaveChanges();
            return Json(new { success = true, message = "Operation successful." });
        }
        #endregion

    }
}

