using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using QuickCart.Areas.Customer.Controllers;
using QuickCart.DataAccess.Repository.IRepository;
using QuickCart.Models;
using Xunit;

namespace QuickCart.Tests
{
    public class CartControllerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IShoppingCartRepository> _mockCartRepo;
        private readonly CartController _controller;

        public CartControllerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockCartRepo = new Mock<IShoppingCartRepository>();
            _mockUnitOfWork.Setup(u => u.ShoppingCart).Returns(_mockCartRepo.Object);

            _controller = new CartController(_mockUnitOfWork.Object);
        }

        private void SetUserContext(string userId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, "testuser@example.com")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        [Fact]
        public async Task Plus_WhenCartBelongsToAnotherUser_ReturnsForbidResult()
        {
            // Arrange: Authenticated as "user-123", but cart belongs to "victim-456" (BOLA attempt)
            SetUserContext("user-123");
            var victimCart = new ShoppingCart
            {
                Id = 10,
                ApplicationUserId = "victim-456",
                ProductId = 1,
                Count = 2
            };

            _mockCartRepo.Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<ShoppingCart, bool>>>(),
                It.IsAny<string>(),
                true,
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(victimCart);

            // Act
            var result = await _controller.Plus(10);

            // Assert: Must be rejected with ForbidResult
            Assert.IsType<ForbidResult>(result);
            _mockCartRepo.Verify(r => r.Update(It.IsAny<ShoppingCart>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Plus_WhenCartBelongsToCurrentUser_IncrementsCountAndRedirects()
        {
            // Arrange
            SetUserContext("user-123");
            var userCart = new ShoppingCart
            {
                Id = 10,
                ApplicationUserId = "user-123",
                ProductId = 1,
                Count = 2
            };

            _mockCartRepo.Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<ShoppingCart, bool>>>(),
                It.IsAny<string>(),
                true,
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(userCart);

            // Act
            var result = await _controller.Plus(10);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal(3, userCart.Count);
            _mockCartRepo.Verify(r => r.Update(userCart), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Minus_WhenCartBelongsToAnotherUser_ReturnsForbidResult()
        {
            // Arrange
            SetUserContext("user-123");
            var victimCart = new ShoppingCart
            {
                Id = 15,
                ApplicationUserId = "victim-456",
                ProductId = 2,
                Count = 5
            };

            _mockCartRepo.Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<ShoppingCart, bool>>>(),
                It.IsAny<string>(),
                true,
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(victimCart);

            // Act
            var result = await _controller.Minus(15);

            // Assert
            Assert.IsType<ForbidResult>(result);
            _mockCartRepo.Verify(r => r.Remove(It.IsAny<ShoppingCart>()), Times.Never);
        }

        [Fact]
        public async Task Remove_WhenCartBelongsToAnotherUser_ReturnsForbidResult()
        {
            // Arrange
            SetUserContext("attacker-999");
            var victimCart = new ShoppingCart
            {
                Id = 20,
                ApplicationUserId = "victim-456",
                ProductId = 3,
                Count = 1
            };

            _mockCartRepo.Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<ShoppingCart, bool>>>(),
                It.IsAny<string>(),
                true,
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(victimCart);

            // Act
            var result = await _controller.Remove(20);

            // Assert
            Assert.IsType<ForbidResult>(result);
            _mockCartRepo.Verify(r => r.Remove(It.IsAny<ShoppingCart>()), Times.Never);
        }
    }
}
