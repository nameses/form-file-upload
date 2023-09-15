using azure_form_file_upload.Settings.Configuration;
using BlobTrigger.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using webapi.Settings.Configuration;

namespace WebApiTest.Tests
{
    public class BlobEmailTriggerTests
    {

        public BlobEmailTriggerTests()
        {

        }


        [Fact]
        public void GenerateUriWithSASToken_ReturnsValidUrlAndSASToken()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("./Settings/appsecrets.json") // Provide the path to your configuration file
                .Build();

            var azureBlobStorageConfig = configuration.GetSection("Azure").GetSection("BlobStorage").Get<AzureBlobStorageConfig>()!;

            var loggerMock = new Mock<ILogger<EmailSenderService>>();
            var service = new EmailSenderService(loggerMock.Object);
            var uri = new Uri("https://storageresourcegroup0.blob.core.windows.net/file-upload/test.txt");
            var blobName = "test.txt";

            Environment.SetEnvironmentVariable("BlobConnectionString", azureBlobStorageConfig.ConnectionString);

            var res = service.GenerateUriWithSASToken(uri, blobName);

            Assert.NotNull(res);
            Assert.Contains(uri.ToString(), res.Split("?"));
        }

        [Fact]
        public async Task SendEmail_ReturnsValidResultNotThrowsExceptions()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("./Settings/appsecrets.json")
                .AddJsonFile("./Settings/emailSettings.json")
                .Build();

            var azureBlobStorageConfig = configuration.GetSection("Azure").GetSection("BlobStorage").Get<AzureBlobStorageConfig>()!;
            var emailConfig = configuration.GetSection("Email").Get<EmailConfig>()!;

            var loggerMock = new Mock<ILogger<EmailSenderService>>();
            var service = new EmailSenderService(loggerMock.Object);
            var uri = new Uri("https://storageresourcegroup0.blob.core.windows.net/file-upload/04f768-20230914202712889.docx");
            var blobName = "04f768-20230914202712889.docx";
            var dict = new Dictionary<string, string>()
                {
                    { "email", "test@gmail.com" },
                    { "username", "username" },
                    { "filename", "filename.docx" }
                };

            Environment.SetEnvironmentVariable("BlobConnectionString", azureBlobStorageConfig.ConnectionString);
            Environment.SetEnvironmentVariable("EmailHost", emailConfig.EmailHost);
            Environment.SetEnvironmentVariable("EmailPort", emailConfig.EmailPort);
            Environment.SetEnvironmentVariable("EmailUsername", emailConfig.EmailUsername);
            Environment.SetEnvironmentVariable("EmailPassword", emailConfig.EmailPassword);
            Environment.SetEnvironmentVariable("EmailFrom", emailConfig.EmailFrom);


            Func<Task> action = async () => await service.SendEmail(dict, blobName, uri);

            // Assert
            await action.Should().NotThrowAsync();
        }
    }
}
