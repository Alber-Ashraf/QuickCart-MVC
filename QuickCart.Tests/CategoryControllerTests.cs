using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using QuickCart.Areas.Admin.Controllers;
using QuickCart.DataAccess.Repository.IRepository;
using QuickCart.Models;
using Xunit;

namespace QuickCart.Tests
{
    public class CategoryControllerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ICategoryRepository> _mockCategoryRepo;
        private readonly CategoryController _controller;

        public CategoryControllerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockCategoryRepo = new Mock<ICategoryRepository>();
            _mockUnitOfWork.Setup(u => u.Category).Returns(_mockCategoryRepo.Object);

            _controller = new CategoryController(_mockUnitOfWork.Object);

            var httpContext = new DefaultHttpContext();
            _controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
        }

        [Fact]
        public async Task Create_WhenNameMatchesDisplayOrder_AddsModelErrorAndReturnsView()
        {
            // Arrange
            var category = new Category
            {
                Name = "5",
                DisplayOrder = 5
            };

            // Act
            var result = await _controller.Create(category);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.True(_controller.ModelState.ContainsKey("Name"));
            _mockCategoryRepo.Verify(r => r.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Create_WhenValid_AddsCategoryAndRedirects()
        {
            // Arrange
            var category = new Category
            {
                Name = "Electronics",
                DisplayOrder = 1
            };

            // Act
            var result = await _controller.Create(category);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            _mockCategoryRepo.Verify(r => r.AddAsync(category, It.IsAny<CancellationToken>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
