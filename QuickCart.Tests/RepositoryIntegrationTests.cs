using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuickCart.Data;
using QuickCart.DataAccess.Repository;
using QuickCart.Models;
using Xunit;

namespace QuickCart.Tests
{
    public class RepositoryIntegrationTests
    {
        private DbContextOptions<QuickCartDbContext> CreateNewContextOptions()
        {
            return new DbContextOptionsBuilder<QuickCartDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task Repository_AddAndGetAsync_SavesAndRetrievesEntity()
        {
            // Arrange
            var options = CreateNewContextOptions();
            using var db = new QuickCartDbContext(options);
            var unitOfWork = new UnitOfWork(db);

            var category = new Category
            {
                Name = "History",
                DisplayOrder = 10
            };

            // Act
            await unitOfWork.Category.AddAsync(category);
            await unitOfWork.SaveAsync();

            var retrieved = await unitOfWork.Category.GetAsync(c => c.Name == "History");

            // Assert
            Assert.NotNull(retrieved);
            Assert.Equal("History", retrieved.Name);
            Assert.Equal(10, retrieved.DisplayOrder);
        }

        [Fact]
        public async Task Repository_GetAllAsync_FiltersCorrectly()
        {
            // Arrange
            var options = CreateNewContextOptions();
            using var db = new QuickCartDbContext(options);
            var unitOfWork = new UnitOfWork(db);

            await unitOfWork.Category.AddAsync(new Category { Name = "Science", DisplayOrder = 1 });
            await unitOfWork.Category.AddAsync(new Category { Name = "Art", DisplayOrder = 2 });
            await unitOfWork.Category.AddAsync(new Category { Name = "Sci-Fi", DisplayOrder = 3 });
            await unitOfWork.SaveAsync();

            // Act
            var results = await unitOfWork.Category.GetAllAsync(c => c.Name.StartsWith("Sci"));

            // Assert
            Assert.Equal(2, results.Count());
        }

        [Fact]
        public async Task Repository_Remove_DeletesEntityAcrossContexts()
        {
            // Arrange
            var options = CreateNewContextOptions();
            using (var writeDb = new QuickCartDbContext(options))
            {
                var writeUow = new UnitOfWork(writeDb);
                await writeUow.Category.AddAsync(new Category { Name = "Temporary", DisplayOrder = 99 });
                await writeUow.SaveAsync();
            }

            // Act: In a new request context, fetch tracked and delete
            using (var deleteDb = new QuickCartDbContext(options))
            {
                var deleteUow = new UnitOfWork(deleteDb);
                var toDelete = await deleteUow.Category.GetAsync(c => c.Name == "Temporary", tracked: true);
                Assert.NotNull(toDelete);

                deleteUow.Category.Remove(toDelete);
                await deleteUow.SaveAsync();
            }

            // Assert: In a subsequent context, entity is gone
            using (var verifyDb = new QuickCartDbContext(options))
            {
                var verifyUow = new UnitOfWork(verifyDb);
                var remaining = await verifyUow.Category.GetAsync(c => c.Name == "Temporary");
                Assert.Null(remaining);
            }
        }
    }
}
