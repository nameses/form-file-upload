using Azure.Storage.Blobs;
using azure_form_file_upload.Settings.Configuration;
using Microsoft.Extensions.Options;
using System.Web;
using webapi.DTO;

namespace webapi.Services
{
    public class FileUploadService
    {
        private readonly ILogger<FileUploadService> _logger;
        private readonly IOptions<AzureBlobStorageConfig> _config;

        public FileUploadService(ILogger<FileUploadService> logger, IOptions<AzureBlobStorageConfig> config)
        {
            _logger = logger;
            _config = config;
        }

        public async Task<string> FileUpload(FileUploadDTO fileUploadDTO)
        {
            var generatedBlobName = GenerateBlobName();

            var container = new BlobContainerClient(_config.Value.ConnectionString, _config.Value.ContainerName);
            var blob = container.GetBlobClient(generatedBlobName);
            var filename = fileUploadDTO.File?.FileName!;

            if (fileUploadDTO.File != null)
            {
                using (Stream stream = fileUploadDTO.File.OpenReadStream())
                {
                    await blob.UploadAsync(stream, true);
                }

                var filename_encoded = HttpUtility.UrlEncode(filename);

                await blob.SetMetadataAsync(new Dictionary<string, string>
                {
                    { "email", fileUploadDTO.Email! },
                    { "username", fileUploadDTO.Username! },
                    { "filename", filename_encoded }
                });

            }
            else
                throw new NullReferenceException(nameof(fileUploadDTO.File));

            _logger.LogInformation("File successfully uploaded");
            return generatedBlobName;
        }

        //function that generate unique names to avoid overwriting files
        private static string GenerateBlobName()
        {
            string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
            string randomStr = Guid.NewGuid().ToString("N")[..6];

            return $"{randomStr}-{timestamp}.docx";
        }
    }
}