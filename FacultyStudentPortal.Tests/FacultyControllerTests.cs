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

            HFService dummyHfService = null;

            // Now create the controller with mocks
            var controller = new FacultyController(mockDb.Object, mockEnv.Object, dummyHfService);

            // Act
            var result = controller.FacultyDashboard();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void CreateAssignment_Get_Returns_View()
        {
            var mockDb = new Mock<IDbConnection>();
            var mockEnv = new Mock<IWebHostEnvironment>();
            HFService dummyService = null;

            var controller = new FacultyController(mockDb.Object, mockEnv.Object, dummyService);

            var result = controller.CreateAssignment();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task CreateAssignment_Post_InvalidModel_Returns_View()
        {
            var mockDb = new Mock<IDbConnection>();
            var mockEnv = new Mock<IWebHostEnvironment>();
            HFService dummyService = null;

            var controller = new FacultyController(mockDb.Object, mockEnv.Object, dummyService);
            controller.ModelState.AddModelError("Title", "Required");

            var model = new CreateAssignmentViewModel(); // Invalid because "Title" is missing

            var result = await controller.CreateAssignment(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
        }

        [Fact]
        public async Task CreateAssignment_Post_ValidModel_Redirects()
        {
            var mockDb = new Mock<IDbConnection>();
            var mockEnv = new Mock<IWebHostEnvironment>();
            var mockCommand = new Mock<IDbCommand>();
            HFService dummyService = null;

            mockDb.Setup(db => db.ExecuteAsync(It.IsAny<string>(), It.IsAny<object>(), null, null, null))
                  .ReturnsAsync(1); // pretend DB insert was successful

            var controller = new FacultyController(mockDb.Object, mockEnv.Object, dummyService);

            var model = new CreateAssignmentViewModel
            {
                Title = "Valid Title",
                Description = "Desc",
                DueDate = DateTime.Now.AddDays(2)
            };

            var result = await controller.CreateAssignment(model);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ViewAssignments", redirectResult.ActionName);
        }



    }
}