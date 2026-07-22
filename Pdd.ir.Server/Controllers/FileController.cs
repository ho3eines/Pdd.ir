using Microsoft.AspNetCore.Mvc;

namespace Pdd.ir.Server.Controllers
{
    [ApiController]
    [Route("api/file")]
    public class FileController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public FileController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromBody] FileUploadRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Base64Data))
                    return BadRequest(new { success = false, message = "No file data" });

                // Parse Base64
                var base64 = request.Base64Data;
                if (base64.Contains(","))
                    base64 = base64.Split(',')[1]; // Remove data:image/png;base64, prefix

                var fileBytes = Convert.FromBase64String(base64);

                // Validate size (10MB max)
                if (fileBytes.Length > 10 * 1024 * 1024)
                    return BadRequest(new { success = false, message = "File too large" });

                // Generate unique filename
                var extension = GetExtension(request.ContentType);
                var fileName = $"{Guid.NewGuid():N}{extension}";

                // Determine folder
                var folder = request.Folder ?? "uploads";
                var uploadPath = Path.Combine(_env.WebRootPath, "uploads", folder);
                Directory.CreateDirectory(uploadPath);

                // Save file
                var filePath = Path.Combine(uploadPath, fileName);
                await System.IO.File.WriteAllBytesAsync(filePath, fileBytes);

                // Build URL
                var url = $"/uploads/{folder}/{fileName}";

                return Ok(new { success = true, data = new { url, fileName } });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("upload-form")]
        public async Task<IActionResult> UploadForm(IFormFile file, [FromQuery] string? folder)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { success = false, message = "No file" });

                // Validate size
                if (file.Length > 100 * 1024 * 1024) // 100MB
                    return BadRequest(new { success = false, message = "File too large" });

                // Generate filename
                var extension = Path.GetExtension(file.FileName);
                var fileName = $"{Guid.NewGuid():N}{extension}";

                // Save
                var uploadFolder = folder ?? "uploads";
                var uploadPath = Path.Combine(_env.WebRootPath, "uploads", uploadFolder);
                Directory.CreateDirectory(uploadPath);

                var filePath = Path.Combine(uploadPath, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);

                var url = $"/uploads/{uploadFolder}/{fileName}";

                return Ok(new { success = true, data = new { url, fileName } });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> Delete([FromBody] FileDeleteRequest request)
        {
            try
            {
                var filePath = Path.Combine(_env.WebRootPath, request.Url.TrimStart('/'));

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        private string GetExtension(string contentType)
        {
            return contentType switch
            {
                "image/png" => ".png",
                "image/jpeg" => ".jpg",
                "image/gif" => ".gif",
                "image/webp" => ".webp",
                "application/pdf" => ".pdf",
                _ => ".bin"
            };
        }
    }

    public class FileUploadRequest
    {
        public string Base64Data { get; set; } = "";
        public string FileName { get; set; } = "";
        public string ContentType { get; set; } = "";
        public string? Folder { get; set; }
    }

    public class FileDeleteRequest
    {
        public string Url { get; set; } = "";
    }
}
