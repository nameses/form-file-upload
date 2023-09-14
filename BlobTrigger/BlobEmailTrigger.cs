using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;

namespace BlobTrigger
{
    public class BlobEmailTrigger
    {
        private static readonly string BlobConnectionString = Environment.GetEnvironmentVariable("storageresourcegroup0_STORAGE");
        private static readonly string EmailHost = Environment.GetEnvironmentVariable("EmailHost");
        private static readonly int EmailPort = int.Parse(Environment.GetEnvironmentVariable("EmailPort"));
        private static readonly string EmailUsername = Environment.GetEnvironmentVariable("EmailUsername");
        private static readonly string EmailPassword = Environment.GetEnvironmentVariable("EmailPassword");
        private static readonly string EmailFrom = Environment.GetEnvironmentVariable("EmailFrom");

        [FunctionName("BlobEmailTrigger")]
        public async Task RunAsync([BlobTrigger("file-upload/{name}", Connection = "storageresourcegroup0_STORAGE")] Stream myBlob,
            string name, IDictionary<string, string> metadata, ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");
            log.LogInformation($"{myBlob}");
            try
            {
                //connection
                BlobServiceClient blobServiceClient = new BlobServiceClient(BlobConnectionString);
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("file-upload");
                BlobClient blobClient = containerClient.GetBlobClient(name);

                //get email address and username
                metadata.TryGetValue("email", out var email);
                metadata.TryGetValue("username", out var username);
                metadata.TryGetValue("filename", out var filename);
                filename = HttpUtility.UrlDecode(filename);

                Uri blobUri = blobClient.Uri;

                // Generate SAS token with 1 hour expiration.
                DateTimeOffset sasExpiration = DateTime.Now.AddHours(1);
                string sasToken = blobClient.GenerateSasUri(BlobSasPermissions.Read, sasExpiration).Query;
                log.LogInformation($"{sasToken}");
                // Send an email notification with URL
                using (var client = new SmtpClient(EmailHost, EmailPort))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(EmailUsername, EmailPassword);

                    using (var message = new MailMessage(from: EmailFrom, to: email))
                    {
                        message.Subject = "File Uploaded Notification";
                        message.Body = $"Welcome, {username}! File '{filename}' was successfully uploaded.\n\nSecure URL: {blobUri}{sasToken}";

                        client.Send(message);
                        log.LogInformation($"Email to address \'{email}\' send successfully.");
                    }
                }

                log.LogInformation($"File '{name}' uploaded successfully.");
            }
            catch (Exception ex)
            {
                log.LogError("Error processing blob");
                log.LogError($"{ex}");
            }
        }
    }
}
