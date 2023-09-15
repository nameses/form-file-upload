using BlobTrigger.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AzureBlobTrigger
{
    public class BlobEmailTrigger
    {
        private readonly ILogger<BlobEmailTrigger> _logger;
        private readonly EmailSenderService _emailSenderService;


        public BlobEmailTrigger(ILogger<BlobEmailTrigger> logger, EmailSenderService emailSenderService)
        {
            _logger = logger;
            _emailSenderService=emailSenderService;
        }

        [Function("BlobEmailTrigger")]
        public async Task RunAsync([BlobTrigger("file-upload/{name}", Connection = "BlobConnectionString")] string myBlob,
            string name,
            IDictionary<string, string> metadata,
            Uri uri)
        {
            _logger.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");
            try
            {
                if (metadata.ContainsKey("email") && metadata.ContainsKey("username") && metadata.ContainsKey("filename"))
                {
                    await _emailSenderService.SendEmail(metadata, name, uri);
                }
                else
                {
                    _logger.LogError($"File {name} without metadata");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error processing blob");
                _logger.LogError($"{ex}");
            }
        }
    }
}
