using azure_form_file_upload.Controllers;
using azure_form_file_upload.Settings.Configuration;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using webapi.Services;
using WebApiTest.Shared;

namespace WebApiTest.Tests
{
    public class AzureBlobStorageControllerTests
    {
        private string EmailTest { get; } = "test@example.com";
        private string UsernameTest { get; } = "testusername";
        private AzureBlobStorageConfig? azureBlobStorageConfig { get; set; }
        public AzureBlobStorageControllerTests()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("./Settings/appsecrets.json") // Provide the path to your configuration file
                .Build();

            azureBlobStorageConfig = configuration.GetSection("Azure").GetSection("TestBlobStorage").Get<AzureBlobStorageConfig>()!;
        }

        [Theory]
        [InlineData("testusername", "test@example.com", null, "File", "Invalid file")]
        [InlineData("testusername", "test@example.com", "file.pdf", "File", "Invalid file extension")]
        [InlineData(null, "test@example.com", "file.docx", "Username", "Username is required")]
        [InlineData("testusername", null, "file.docx", "Email", "Email is required")]
        [InlineData("testusername", "in...valid@emailregex@dot", "file.docx", "Email", "Email is not in valid format")]
        public async Task Upload_WithInvalidInput_ReturnsBadRequestWithModelError(
            string username, string email, string fileName, string expectedModelTypeError, string expectedErrorMessage)
        {
            // Arrange
            var fileDTO = DependencyFaker.CreateValidFileUploadDTO(email, username, fileName, "TEST content from this string");
            var formCollection = DependencyFaker.CreateFormCollection(fileDTO);

            FileUploadService fileUploadService = DependencyFaker.CreateFileUploadService();
            var loggerMock = new Mock<ILogger<AzureBlobStorageController>>();

            var controller = new AzureBlobStorageController(loggerMock.Object, fileUploadService)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        Request = { Form = formCollection }
                    }
                }
            };

            // Act
            var result = await controller.Upload();

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var modelState = badRequestResult.Value as SerializableError;

            Assert.NotNull(modelState);
            Assert.True(modelState.ContainsKey(expectedModelTypeError));
            var errors = (string[])modelState[expectedModelTypeError];

            Assert.Contains(expectedErrorMessage, errors);
        }

        [Fact]
        public async Task TestAzureBlobStorageController_Upload_UploadSuccessful()
        {
            var fileDTO = DependencyFaker.CreateValidFileUploadDTO(EmailTest, UsernameTest, "test.docx", "TEST content from this string");

            var formCollection = DependencyFaker.CreateFormCollection(fileDTO);

            FileUploadService fileUploadService = DependencyFaker.CreateFileUploadService();
            var loggerMock = new Mock<ILogger<AzureBlobStorageController>>();

            var controller = new AzureBlobStorageController(loggerMock.Object, fileUploadService)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        Request = { Form = formCollection }
                    }
                }
            };

            // Act
            var result = await controller.Upload();

            // Assert
            result.Should().BeOfType<OkResult>();
        }
    }
}
