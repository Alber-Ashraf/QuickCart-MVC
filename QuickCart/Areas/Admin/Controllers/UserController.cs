using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuickCart.Data;
using QuickCart.DataAccess.Repository.IRepository;
using QuickCart.Models;
using QuickCart.Models.ViewModels;
using QuickCart.Utility;

namespace QuickCart.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
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
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            return Json(new { sucess = true, massege = "Delete Sucsess" });
        }
        #endregion

    }
}

