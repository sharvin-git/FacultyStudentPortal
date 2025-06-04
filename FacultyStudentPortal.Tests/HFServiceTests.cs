using Moq;
using Moq.Protected;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FacultyStudentPortal.Tests
{
    public class HFServiceTests
    {
        [Fact]
        public async Task GenerateFeedbackFromContentAsync_ReturnsExpectedFeedback()
        {
            // Arrange
            var expectedText = "The student has shown good understanding...";
            var fakeJson = "[{\"generated_text\": \"" + expectedText + "\"}]";

            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(fakeJson)
                });

            var client = new HttpClient(mockHandler.Object);

            var service = new HFService(client, "fake-token");

            // Act
            var result = await service.GenerateFeedbackFromContentAsync("Some prompt");

            // Assert
            Assert.Equal(expectedText, result);
        }
    }

}
