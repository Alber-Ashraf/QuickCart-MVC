using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using QuickCart.Areas.Admin.Controllers;
using QuickCart.DataAccess.Repository.IRepository;
using QuickCart.Models;
using QuickCart.Utility;
using Xunit;

namespace QuickCart.Tests
{
    public class OrderControllerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IOrderHeaderRepository> _mockOrderHeaderRepo;
        private readonly Mock<IOrderDetailRepository> _mockOrderDetailRepo;
        private readonly OrderController _controller;

        public OrderControllerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockOrderHeaderRepo = new Mock<IOrderHeaderRepository>();
            _mockOrderDetailRepo = new Mock<IOrderDetailRepository>();

            _mockUnitOfWork.Setup(u => u.OrderHeader).Returns(_mockOrderHeaderRepo.Object);
            _mockUnitOfWork.Setup(u => u.OrderDetails).Returns(_mockOrderDetailRepo.Object);

            _controller = new OrderController(_mockUnitOfWork.Object);
        }

        private void SetUserContext(string userId, string role = SD.Role_Customer)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Role, role)
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        [Fact]
        public async Task Details_WhenRegularCustomerViewsAnotherUsersOrder_ReturnsForbid()
        {
            // Arrange: Authenticated as customer "user-1", order belongs to "user-2"
            SetUserContext("user-1", SD.Role_Customer);

            var foreignOrder = new OrderHeader
            {
                OrderHeaderId = 42,
                ApplicationUserId = "user-2"
            };

            _mockOrderHeaderRepo.Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<OrderHeader, bool>>>(),
                "ApplicationUser",
                false,
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(foreignOrder);

            // Act
            var result = await _controller.Details(42);

            // Assert: Must be rejected with ForbidResult
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task Details_WhenAdminViewsAnyOrder_ReturnsViewWithOrder()
        {
            // Arrange: Authenticated as Admin, viewing customer's order
            SetUserContext("admin-999", SD.Role_Admin);

            var customerOrder = new OrderHeader
            {
                OrderHeaderId = 42,
                ApplicationUserId = "customer-1"
            };

            _mockOrderHeaderRepo.Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<OrderHeader, bool>>>(),
                "ApplicationUser",
                false,
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(customerOrder);

            _mockOrderDetailRepo.Setup(r => r.GetAllAsync(
                It.IsAny<Expression<Func<OrderDetail, bool>>>(),
                "Product",
                true,
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<OrderDetail>());

            // Act
            var result = await _controller.Details(42);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.Model);
        }
    }
}
