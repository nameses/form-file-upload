using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;
using System.Web;

namespace BlobTrigger.Services
{
    public class EmailSenderService
    {
        private readonly ILogger<EmailSenderService> _logger;

        public EmailSenderService(ILogger<EmailSenderService> logger)
        {
            _logger=logger;
        }

        public async Task SendEmail(IDictionary<string, string> metadata, string blobName, Uri uri)
        {
            //get email address and username
            metadata.TryGetValue("email", out var email);
            metadata.TryGetValue("username", out var username);
            metadata.TryGetValue("filename", out var filename);
            filename = HttpUtility.UrlDecode(filename);

            //generate sas token and get full url
            var urlWithSasToken = GenerateUriWithSASToken(uri, blobName);

            // Send an email notification with smtp client
            using (var client = new SmtpClient(
                Environment.GetEnvironmentVariable("EmailHost"),
                int.Parse(Environment.GetEnvironmentVariable("EmailPort"))))
            {
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(
                    Environment.GetEnvironmentVariable("EmailUsername"),
                    Environment.GetEnvironmentVariable("EmailPassword")
                );

                using (var message = new MailMessage(from: Environment.GetEnvironmentVariable("EmailFrom"), to: email))
                {
                    message.Subject = "File Uploaded Notification";
                    message.Body = $"Welcome, {username}! File '{filename}' was successfully uploaded.\n\nSecure URL: {urlWithSasToken}";

                    client.Send(message);
                    _logger.LogInformation($"Email to address \'{email}\' send successfully.");
                }
            }
        }

        public string GenerateUriWithSASToken(Uri uri, string blobName)
        {
            var containerClient = new BlobContainerClient(
                Environment.GetEnvironmentVariable("BlobConnectionString"),
                "file-upload"
            );
            var blobClient = containerClient.GetBlobClient(blobName);

            // Generate SAS token with 1 hour expiration.
            DateTimeOffset sasExpiration = DateTime.Now.AddHours(1);
            string sasToken = blobClient.GenerateSasUri(BlobSasPermissions.Read, sasExpiration).Query;

            return $"{uri}{sasToken}";
        }
    }
}
