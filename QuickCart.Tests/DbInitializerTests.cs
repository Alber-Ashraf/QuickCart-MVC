using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using QuickCart.Data;
using QuickCart.DataAccess.DbInitializer;
using QuickCart.Models;
using Xunit;

namespace QuickCart.Tests
{
    public class DbInitializerTests
    {
        private QuickCartDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<QuickCartDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new QuickCartDbContext(options);
        }

        private Mock<UserManager<IdentityUser>> CreateMockUserManager()
        {
            var store = new Mock<IUserStore<IdentityUser>>();
            var userManager = new Mock<UserManager<IdentityUser>>(store.Object, null, null, null, null, null, null, null, null);
            userManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((IdentityUser?)null);
            userManager.Setup(m => m.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            userManager.Setup(m => m.AddToRoleAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            return userManager;
        }

        private Mock<RoleManager<IdentityRole>> CreateMockRoleManager()
        {
            var store = new Mock<IRoleStore<IdentityRole>>();
            var roleManager = new Mock<RoleManager<IdentityRole>>(store.Object, null, null, null, null);
            roleManager.Setup(m => m.RoleExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(true);
            return roleManager;
        }

        private IConfiguration CreateMockConfiguration()
        {
            var inMemorySettings = new Dictionary<string, string>
            {
                {"AdminUser:Email", "admin@flyleaf.com"},
                {"AdminUser:Password", "AdminPassword123!"}
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();
        }

        [Fact]
        public void Initialize_SeedsRealCategoriesAndBooks()
        {
            using var db = CreateInMemoryDbContext();
            var userManager = CreateMockUserManager();
            var roleManager = CreateMockRoleManager();
            var config = CreateMockConfiguration();

            var initializer = new DbInitializer(userManager.Object, roleManager.Object, db, config);

            initializer.Initialize();

            // Verify categories are seeded
            Assert.True(db.Categories.Count() >= 5);
            Assert.Contains(db.Categories, c => c.Name == "Classics");
            Assert.Contains(db.Categories, c => c.Name == "Sci-Fi & Fantasy");
            Assert.Contains(db.Categories, c => c.Name == "Technology");
            Assert.Contains(db.Categories, c => c.Name == "Self-Development");

            // Verify real books are seeded
            var products = db.Products.Include(p => p.ProductImages).ToList();
            Assert.Equal(12, products.Count);

            // Verify specific classic titles exist with real metadata
            var gatsby = products.FirstOrDefault(p => p.Title == "The Great Gatsby");
            Assert.NotNull(gatsby);
            Assert.Equal("F. Scott Fitzgerald", gatsby.Author);
            Assert.Equal("978-0743273565", gatsby.ISBN);
            Assert.Single(gatsby.ProductImages);
            Assert.Equal("/images/books/the-great-gatsby.jpg", gatsby.ProductImages.First().ImageUrl);

            var cleanCode = products.FirstOrDefault(p => p.Title.StartsWith("Clean Code"));
            Assert.NotNull(cleanCode);
            Assert.Equal("Robert C. Martin", cleanCode.Author);
            Assert.Equal("978-0132350884", cleanCode.ISBN);
            Assert.Equal("/images/books/clean-code.jpg", cleanCode.ProductImages.First().ImageUrl);
        }

        [Fact]
        public void Initialize_IsIdempotent_DoesNotDuplicateBooksOnSubsequentRuns()
        {
            using var db = CreateInMemoryDbContext();
            var userManager = CreateMockUserManager();
            var roleManager = CreateMockRoleManager();
            var config = CreateMockConfiguration();

            var initializer = new DbInitializer(userManager.Object, roleManager.Object, db, config);

            // Run twice
            initializer.Initialize();
            initializer.Initialize();

            Assert.Equal(12, db.Products.Count());
            Assert.Equal(12, db.ProductImages.Count());
        }

        [Fact]
        public void Initialize_RemovesUnusedPlaceholderDemoBooks()
        {
            using var db = CreateInMemoryDbContext();
            var userManager = CreateMockUserManager();
            var roleManager = CreateMockRoleManager();
            var config = CreateMockConfiguration();

            // Add an old placeholder book
            var dummyCategory = new Category { Name = "Action", DisplayOrder = 99 };
            db.Categories.Add(dummyCategory);
            db.SaveChanges();

            db.Products.Add(new Product
            {
                Title = "Fortune of Time",
                Author = "Billy Spark",
                Description = "Praesent vitae sodales libero...",
                ISBN = "SWD9999001",
                ListPrice = 99,
                Price = 90,
                Price50 = 85,
                Price100 = 80,
                CategoryId = dummyCategory.Id
            });
            db.SaveChanges();

            var initializer = new DbInitializer(userManager.Object, roleManager.Object, db, config);
            initializer.Initialize();

            // Placeholder should be cleaned up
            Assert.DoesNotContain(db.Products, p => p.Title == "Fortune of Time");
            // Real books should be present
            Assert.Equal(12, db.Products.Count());
        }
    }
}
