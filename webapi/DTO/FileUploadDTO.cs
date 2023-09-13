namespace webapi.DTO
{
    public class FileUploadDTO
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public IFormFile? File { get; set; }
    }
}
