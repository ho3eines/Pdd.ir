using Microsoft.AspNetCore.Mvc;

namespace Pdd.ir.Server.Controllers
{
    [ApiController]
    [Route("api/upload")]
    public class UploadController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public UploadController(IWebHostEnvironment env)
        {
            _env = env;
        }

        /// <summary>
        /// آپلود فایل و برگرداندن GUID
        /// POST /api/upload
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Upload([FromBody] UploadRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Base64Data))
                    return BadRequest(new { success = false, message = "No file data" });

                // استخراج Base64
                var base64 = request.Base64Data;
                if (base64.Contains(","))
                    base64 = base64.Split(',')[1];

                var fileBytes = Convert.FromBase64String(base64);

                // بررسی حجم (حداکثر 10MB)
                if (fileBytes.Length > 10 * 1024 * 1024)
                    return BadRequest(new { success = false, message = "File too large" });

                // ساخت GUID
                var guid = Guid.NewGuid().ToString("N");
                var extension = GetExtension(request.ContentType);
                var fileName = $"{guid}{extension}";

                // پوشه ذخیره
                var folder = request.Folder ?? "uploads";
                var uploadPath = Path.Combine(_env.WebRootPath, "uploads", folder);
                Directory.CreateDirectory(uploadPath);

                // ذخیره فایل
                var filePath = Path.Combine(uploadPath, fileName);
                await System.IO.File.WriteAllBytesAsync(filePath, fileBytes);

                // برگرداندن GUID
                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        id = guid,
                        fileName = fileName,
                        contentType = request.ContentType
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// دریافت فایل به صورت Base64 با GUID
        /// GET /api/upload/{id}
        /// </summary>
        [HttpGet("{id}")]
        public IActionResult GetFile(string id)
        {
            try
            {
                var uploadPath = Path.Combine(_env.WebRootPath, "uploads");

                // جستجوی فایل با GUID (شامل زیرپوشه‌ها)
                var files = Directory.GetFiles(uploadPath, $"{id}.*", SearchOption.AllDirectories);
                if (files.Length == 0)
                    return NotFound(new { success = false, message = "File not found" });

                var filePath = files[0];
                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                var base64 = Convert.ToBase64String(fileBytes);
                var contentType = GetContentType(filePath);

                // برگرداندن Base64
                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        id = id,
                        base64 = base64,
                        contentType = contentType
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// حذف فایل با GUID
        /// DELETE /api/upload/{id}
        /// </summary>
        [HttpDelete("{id}")]
        public IActionResult DeleteFile(string id)
        {
            try
            {
                var uploadPath = Path.Combine(_env.WebRootPath, "uploads");

                var files = Directory.GetFiles(uploadPath, $"{id}.*", SearchOption.AllDirectories);
                if (files.Length == 0)
                    return NotFound(new { success = false, message = "File not found" });

                System.IO.File.Delete(files[0]);
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
                "image/jpeg" or "image/jpg" => ".jpg",
                "image/gif" => ".gif",
                "image/webp" => ".webp",
                "application/pdf" => ".pdf",
                _ => ".bin"
            };
        }

        private string GetContentType(string filePath)
        {
            var ext = Path.GetExtension(filePath).ToLowerInvariant();
            return ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".pdf" => "application/pdf",
                _ => "application/octet-stream"
            };
        }
    }

    public class UploadRequest
    {
        public string Base64Data { get; set; } = "";
        public string FileName { get; set; } = "";
        public string ContentType { get; set; } = "";
        public string Folder { get; set; } = "uploads";
    }
}
