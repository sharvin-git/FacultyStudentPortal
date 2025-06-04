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


    }
}