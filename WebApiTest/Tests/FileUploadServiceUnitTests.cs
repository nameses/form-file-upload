using Azure.Storage.Blobs;
using azure_form_file_upload.Settings.Configuration;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using webapi.DTO;
using webapi.Services;
using WebApiTest.Shared;

namespace WebApiTest.Tests
{
    public class FileUploadServiceUnitTests
    {
        private static string EmailTest { get; } = "test@example.com";
        private static string UsernameTest { get; } = "testusername";
        private AzureBlobStorageConfig? azureBlobStorageConfig { get; set; }
        public FileUploadServiceUnitTests()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("./Settings/appsecrets.json") // Provide the path to your configuration file
                .Build();

            azureBlobStorageConfig = configuration.GetSection("Azure").GetSection("TestBlobStorage").Get<AzureBlobStorageConfig>()!;
        }

        [Fact]
        public async Task TestFileUploadToBlobStorage_UploadUnsuccessful()
        {
            // Arrange
            string? resultGeneratedName;

            FileUploadService fileUploadService = DependencyFaker.CreateFileUploadService();
            FileUploadDTO fileUploadDTO = DependencyFaker.CreateNonValidFileUploadDTO(EmailTest, UsernameTest, "testname.docx");
            //create container
            var container = new BlobContainerClient(azureBlobStorageConfig?.ConnectionString, azureBlobStorageConfig?.ContainerName);

            if (!(await container.ExistsAsync()).Value)
                await container.CreateAsync();

            // Act
            Func<Task> action = async () => resultGeneratedName = await fileUploadService.FileUpload(fileUploadDTO);

            // Assert
            await action.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task TestFileUploadToBlobStorage_UploadSuccessful()
        {
            // Arrange
            string? resultGeneratedName = null;

            FileUploadService fileUploadService = DependencyFaker.CreateFileUploadService();
            FileUploadDTO fileUploadDTO = DependencyFaker.CreateValidFileUploadDTO(EmailTest, UsernameTest, "test.docx", "TEST content from this string");
            //create container
            var container = new BlobContainerClient(azureBlobStorageConfig?.ConnectionString, azureBlobStorageConfig?.ContainerName);

            if (!(await container.ExistsAsync()).Value)
                await container.CreateAsync();

            // Act
            Func<Task> action = async () => resultGeneratedName = await fileUploadService.FileUpload(fileUploadDTO);

            // Assert
            await action.Should().NotThrowAsync();

            // Test that a blob was created with the expected metadata and size
            await AssertBlobIsCreatedWithExpectedMetadataAndSize(resultGeneratedName, fileUploadDTO);
        }

        private async Task AssertBlobIsCreatedWithExpectedMetadataAndSize(string? resultGeneratedName, FileUploadDTO fileUploadDTO)
        {
            //create container and assert if container exists
            var container = new BlobContainerClient(azureBlobStorageConfig?.ConnectionString, azureBlobStorageConfig?.ContainerName);

            //assert if blob exists
            var blob = container.GetBlobClient(resultGeneratedName);
            (await blob.ExistsAsync()).Value.Should().Be(true);

            //assert if properties are equal to expected
            var properties = await blob.GetPropertiesAsync();
            var metadata = properties.Value.Metadata;
            var expectedMetadata = new Dictionary<string, string>()
            {
                { "email", EmailTest },
                { "username", UsernameTest },
                { "filename", fileUploadDTO.File!.FileName }
            };
            //assert if properties are equal to expected
            metadata.Should().BeEquivalentTo(expectedMetadata);

            //assert if file length is equal to expected
            properties.Value.ContentLength.Should().Be(fileUploadDTO.File!.Length);

            await blob.DeleteAsync();
            (await blob.ExistsAsync()).Value.Should().Be(false);
        }
    }
}