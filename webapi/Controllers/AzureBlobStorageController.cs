using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using webapi.DTO;
using webapi.Services;

namespace azure_form_file_upload.Controllers
{
    [ApiController]
    [Route("api/azure")]
    public class AzureBlobStorageController : Controller
    {
        private readonly ILogger<AzureBlobStorageController> _logger;
        private readonly FileUploadService _uploadService;

        public AzureBlobStorageController(ILogger<AzureBlobStorageController> logger, FileUploadService uploadService)
        {
            _logger=logger;
            _uploadService=uploadService;
        }

        [HttpPost, DisableRequestSizeLimit]
        [Route("fileupload")]
        public async Task<IActionResult> Upload()
        {
            var formCollection = await Request.ReadFormAsync();
            var f = formCollection.Files.First();

            formCollection.TryGetValue("username", out var username);
            formCollection.TryGetValue("email", out var email);


            var fileDTO = new FileUploadDTO()
            {
                Username = username,
                Email = email,
                File = f
            };

            if (fileDTO.File == null || fileDTO.File.Length == 0 || fileDTO.File.FileName == null)
                ModelState.AddModelError("File", "Invalid file");
            else
            {
                var fileExtension = Path.GetExtension(fileDTO.File.FileName).ToLowerInvariant();

                if (fileExtension!=".docx")
                {
                    ModelState.AddModelError("File", "Invalid file extension");
                }
            }

            if (string.IsNullOrEmpty(fileDTO.Username))
                ModelState.AddModelError("Username", "Username is required");

            if (string.IsNullOrEmpty(fileDTO.Email))
                ModelState.AddModelError("Email", "Email is required");
            else
            {
                var pattern = @"^[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}$";
                var emailRegex = new Regex(pattern, RegexOptions.IgnoreCase);

                if (!emailRegex.IsMatch(fileDTO.Email))
                    ModelState.AddModelError("Email", "Email is not in valid format");
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _logger.LogInformation("File uploading started");

            await _uploadService.FileUpload(fileDTO);

            return Ok();
        }
    }
}
