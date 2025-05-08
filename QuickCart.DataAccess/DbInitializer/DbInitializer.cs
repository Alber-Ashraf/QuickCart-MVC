using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuickCart.Data;
using QuickCart.Models;
using QuickCart.Utility;

namespace QuickCart.DataAccess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly QuickCartDbContext _db;

        public DbInitializer(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            QuickCartDbContext db)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _db = db;
        }
        public void Initialize()
        {
            // Apply any pending migrations
            try
            {
                if (_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            // Seed the database with initial data
            if (!_roleManager.RoleExistsAsync(SD.Role_Customer).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Employee)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Company)).GetAwaiter().GetResult();

                // Create default admin user
                _userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "alberAdmin@gmail.com",
                    Email = "alberAdmin@gmail.com",
                    Name = "Alber",
                    PhoneNumber = "01228342430",
                    Address = "123 Main St",
                    City = "New York",
                    State = "NY",
                    PostalCode = "10001",
                }, "139BeRo!").GetAwaiter().GetResult();

                // Assign the admin role to the default admin user
                ApplicationUser user = _db.ApplicationUsers.FirstOrDefault(u => u.Email == "alberAdmin@gmail.com");
                _userManager.AddToRoleAsync(user, SD.Role_Admin).GetAwaiter().GetResult();
            }

            return;
        }
    }
}
    