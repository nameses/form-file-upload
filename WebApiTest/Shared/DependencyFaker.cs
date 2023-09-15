using azure_form_file_upload.Settings.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Moq;
using System.Text;
using webapi.DTO;
using webapi.Services;

namespace WebApiTest.Shared
{
    public class DependencyFaker
    {
        public static FileUploadService CreateFileUploadService()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("./Settings/appsecrets.json")
                .Build();

            var azureBlobStorageConfig = configuration.GetSection("Azure").GetSection("TestBlobStorage").Get<AzureBlobStorageConfig>()!;

            var configMock = new Mock<IOptions<AzureBlobStorageConfig>>();
            configMock.Setup(config => config.Value).Returns(azureBlobStorageConfig!);

            var loggerMock = new Mock<ILogger<FileUploadService>>();

            return new FileUploadService(loggerMock.Object, configMock.Object);
        }


        public static FileUploadDTO CreateValidFileUploadDTO(string email, string username, string filename, string content)
        {
            return new FileUploadDTO
            {
                File = CreateTestIFormFile(filename, content),
                Email = email,
                Username = username
            };
        }
        public static FileUploadDTO CreateNonValidFileUploadDTO(string email, string username, string filename, string content = "")
        {
            return new FileUploadDTO
            {
                File = CreateTestIFormFile(filename, content),
                Email = email,
                Username = username
            };
        }

        public static IFormFile CreateTestIFormFile(string fileName, string content)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(content);

            return new FormFile(
                baseStream: new MemoryStream(bytes),
                baseStreamOffset: 0,
                length: bytes.Length,
                name: "Data",
                fileName: fileName
            );
        }

        public static IFormCollection CreateFormCollection(FileUploadDTO fileDTO)
        {
            return new FormCollection(
                fields: new Dictionary<string, StringValues> { { "username", fileDTO.Username }, { "email", fileDTO.Email } },
                files: new FormFileCollection { fileDTO.File! }
            );
        }
    }
}
