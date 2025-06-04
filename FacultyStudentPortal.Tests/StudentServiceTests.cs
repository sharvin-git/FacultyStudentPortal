using FacultyStudentPortal.Data.Interfaces;
using FacultyStudentPortal.Services;
using FacultyStudentPortal.ViewModels;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FacultyStudentPortal.Tests
{
    public class StudentServiceTests
    {
        [Fact]
        public async Task GetAllStudentsAsync_ReturnsStudentList()
        {
            // Arrange
            var mockRepo = new Mock<IStudentRepository>();
            var expected = new List<StudentListViewModel>
        {
            new StudentListViewModel { FullName = "Sharvin Shakeer", Email = "sharvin@uni.edu" }
        };

            mockRepo.Setup(r => r.GetAllStudentsAsync()).ReturnsAsync(expected);

            var service = new StudentService(mockRepo.Object);

            // Act
            var result = await service.GetAllStudentsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Sharvin Shakeer", result.First().FullName);
        }
    }
}
