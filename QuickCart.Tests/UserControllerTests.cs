using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using QuickCart.Areas.Admin.Controllers;
using QuickCart.DataAccess.Repository.IRepository;
using QuickCart.Models;
using Xunit;

namespace QuickCart.Tests
{
    public class UserControllerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IApplicationUserRepository> _mockUserRepo;
        private readonly Mock<IRepository<IdentityUserRole<string>>> _mockUserRoleRepo;
        private readonly Mock<IRepository<IdentityRole>> _mockRoleRepo;
        private readonly UserController _controller;

        public UserControllerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockUserRepo = new Mock<IApplicationUserRepository>();
            _mockUserRoleRepo = new Mock<IRepository<IdentityUserRole<string>>>();
            _mockRoleRepo = new Mock<IRepository<IdentityRole>>();

            _mockUnitOfWork.Setup(u => u.ApplicationUsers).Returns(_mockUserRepo.Object);
            _mockUnitOfWork.Setup(u => u.UserRoles).Returns(_mockUserRoleRepo.Object);
            _mockUnitOfWork.Setup(u => u.Roles).Returns(_mockRoleRepo.Object);

            _controller = new UserController(_mockUnitOfWork.Object);
        }

        private void SetAdminContext(string adminId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, adminId),
                new Claim(ClaimTypes.Role, "Admin")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        [Fact]
        public async Task LockUnlock_WhenAdminTriesToLockThemselves_ReturnsErrorJson()
        {
            // Arrange: Admin id is "admin-001", and targets "admin-001"
            SetAdminContext("admin-001");

            // Act
            var result = await _controller.LockUnlock("admin-001");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = JsonSerializer.Serialize(jsonResult.Value);
            using var doc = JsonDocument.Parse(json);

            Assert.False(doc.RootElement.GetProperty("success").GetBoolean());
            Assert.Contains("cannot lock your own", doc.RootElement.GetProperty("message").GetString());
            _mockUnitOfWork.Verify(u => u.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task LockUnlock_WhenTargetingAnotherUser_TogglesLockoutEnd()
        {
            // Arrange
            SetAdminContext("admin-001");
            var targetUser = new ApplicationUser
            {
                Id = "user-999",
                LockoutEnd = null
            };

            _mockUserRepo.Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<ApplicationUser, bool>>>(),
                null,
                true,
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(targetUser);

            // Act
            var result = await _controller.LockUnlock("user-999");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = JsonSerializer.Serialize(jsonResult.Value);
            using var doc = JsonDocument.Parse(json);

            Assert.True(doc.RootElement.GetProperty("success").GetBoolean());
            Assert.NotNull(targetUser.LockoutEnd);
            _mockUnitOfWork.Verify(u => u.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
