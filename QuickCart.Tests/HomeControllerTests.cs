using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using QuickCart.Areas.Customer.Controllers;
using QuickCart.DataAccess.Repository.IRepository;
using QuickCart.Models;
using Xunit;

namespace QuickCart.Tests
{
    public class HomeControllerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IProductRepository> _mockProductRepo;
        private readonly Mock<ICategoryRepository> _mockCategoryRepo;
        private readonly Mock<IShoppingCartRepository> _mockCartRepo;
        private readonly Mock<ILogger<HomeController>> _mockLogger;
        private readonly HomeController _controller;

        public HomeControllerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockProductRepo = new Mock<IProductRepository>();
            _mockCategoryRepo = new Mock<ICategoryRepository>();
            _mockCartRepo = new Mock<IShoppingCartRepository>();
            _mockLogger = new Mock<ILogger<HomeController>>();

            _mockUnitOfWork.Setup(u => u.Product).Returns(_mockProductRepo.Object);
            _mockUnitOfWork.Setup(u => u.Category).Returns(_mockCategoryRepo.Object);
            _mockUnitOfWork.Setup(u => u.ShoppingCart).Returns(_mockCartRepo.Object);

            _controller = new HomeController(_mockLogger.Object, _mockUnitOfWork.Object);

            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            _controller.TempData = tempData;
        }

        [Fact]
        public async Task Index_WithSearchFilter_ReturnsFilteredProducts()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category { Id = 1, Name = "Sci-Fi", DisplayOrder = 1 }
            };

            var products = new List<Product>
            {
                new Product { Id = 1, Title = "Dune", Author = "Frank Herbert", Price = 15, CategoryId = 1 },
                new Product { Id = 2, Title = "Foundation", Author = "Isaac Asimov", Price = 18, CategoryId = 1 },
                new Product { Id = 3, Title = "Clean Code", Author = "Robert Martin", Price = 30, CategoryId = 2 }
            };

            _mockCategoryRepo.Setup(c => c.GetAllAsync(null, null, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(categories);

            _mockProductRepo.Setup(p => p.GetAllAsync(null, "Category,ProductImages", true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(products);

            // Act
            var result = await _controller.Index(search: "Dune", categoryId: null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<Product>>(viewResult.Model);
            Assert.Single(model);
            Assert.Equal("Dune", model[0].Title);
        }

        [Fact]
        public async Task Details_WhenProductDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            _mockProductRepo.Setup(p => p.GetAsync(
                It.IsAny<Expression<Func<Product, bool>>>(),
                "Category,ProductImages",
                false,
                It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product?)null);

            // Act
            var result = await _controller.Details(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        [InlineData(1001)]
        public async Task DetailsPost_WhenQuantityIsInvalid_RedirectsWithError(int invalidCount)
        {
            // Arrange
            var cart = new ShoppingCart
            {
                ProductId = 1,
                Count = invalidCount
            };

            // Act
            var result = await _controller.Details(cart);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirectResult.ActionName);
            Assert.NotNull(_controller.TempData["error"]);
        }
    }
}
