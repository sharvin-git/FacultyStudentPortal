// FacultyControllerTests.cs
using Dapper;
using FacultyStudentPortal.Controllers;
using FacultyStudentPortal.Models;
using FacultyStudentPortal.Services;
using FacultyStudentPortal.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Data;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace FacultyStudentPortal.Tests
{
    public class FacultyControllerTests
    {
        [Fact]
        public void FacultyDashboard_Returns_DashboardView()
        {
            var mockDb = new Mock<IDbConnection>();
            var mockEnv = new Mock<IWebHostEnvironment>();
            //var mockHFService = new Mock<HFService>();

            HFService dummyHfService = null;

            // Now create the controller with mocks
            var controller = new FacultyController(mockDb.Object, mockEnv.Object, dummyHfService);

            // Act
            var result = controller.FacultyDashboard();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task ViewAssignments_ReturnsViewWithAssignments()
        {
            // Arrange
            var mockDb = new Mock<IDbConnection>();
            var mockEnv = new Mock<IWebHostEnvironment>();
            var mockHFService = new Mock<HFService>();

            var expectedAssignments = new List<AssignmentListViewModel>
    {
        new AssignmentListViewModel { AssignmentId = 1, Title = "Test Assignment" }
    };

            // Mock Dapper QueryAsync
            mockDb.Setup(db => db.QueryAsync<AssignmentListViewModel>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                null, null, null))
                .ReturnsAsync(expectedAssignments);

            var controller = new FacultyController(mockDb.Object, mockEnv.Object, mockHFService.Object);

            // Mock identity
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
        new Claim(ClaimTypes.NameIdentifier, "test-user-id")
            }, "mock"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await controller.ViewAssignments();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<AssignmentListViewModel>>(viewResult.Model);
            Assert.Single(model); // or Assert.Equal(expectedAssignments.Count, model.Count());
        }

    }
}