// FacultyControllerTests.cs
using Dapper;
using FacultyStudentPortal.Controllers;
using FacultyStudentPortal.Data.Interfaces;
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
            var mockStudentRepo = new Mock<IStudentRepository>();

            HFService dummyHfService = null;

            // Now create the controller with mocks
            var controller = new FacultyController(mockDb.Object, mockEnv.Object, dummyHfService, mockStudentRepo.Object);

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
            var mockStudentRepo = new Mock<IStudentRepository>();

            var controller = new FacultyController(mockDb.Object, mockEnv.Object, dummyService, mockStudentRepo.Object);

            var result = controller.CreateAssignment();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task CreateAssignment_Post_InvalidModel_Returns_View()
        {
            var mockDb = new Mock<IDbConnection>();
            var mockEnv = new Mock<IWebHostEnvironment>();
            HFService dummyService = null;
            var mockStudentRepo = new Mock<IStudentRepository>();

            var controller = new FacultyController(mockDb.Object, mockEnv.Object, dummyService, mockStudentRepo.Object);
            controller.ModelState.AddModelError("Title", "Required");

            var model = new CreateAssignmentViewModel(); // Invalid because "Title" is missing

            var result = await controller.CreateAssignment(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
        }

        [Fact]
        public async Task StudentListFromSP_Returns_StudentListView()
        {
            // Arrange
            var mockDb = new Mock<IDbConnection>();
            var mockEnv = new Mock<IWebHostEnvironment>();
            HFService dummyService = null;
            var mockStudentRepo = new Mock<IStudentRepository>();

            var mockCommand = new Mock<IDbCommand>();
            var fakeStudents = new List<StudentListViewModel>
    {
        new StudentListViewModel { FullName = "Sharvin", Email = "sharvin@example.com" }
    };

            mockDb.Setup(d => d.QueryAsync<StudentListViewModel>(
                "sp_GetAllStudents",
                null,
                null,
                null,
                CommandType.StoredProcedure)).ReturnsAsync(fakeStudents);

            var controller = new FacultyController(mockDb.Object, mockEnv.Object, dummyService, mockStudentRepo.Object);

            // Act
            var result = await controller.StudentListFromSP();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<StudentListViewModel>>(viewResult.Model);
            Assert.Single(model);
        }




    }
}