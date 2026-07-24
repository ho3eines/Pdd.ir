using Microsoft.AspNetCore.Mvc;

namespace Pdd.ir.Server.Controllers
{
    [ApiController]
    [Route("api/image")]
    public class ImageController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public ImageController(IWebHostEnvironment env)
        {
            _env = env;
        }

        /// <summary>
        /// Get image as Base64 for display
        /// GET /api/image/{path}
        /// </summary>
        [HttpGet("{*path}")]
        public IActionResult GetImage(string path)
        {
            try
            {
                var filePath = Path.Combine(_env.WebRootPath, path);
                
                if (!System.IO.File.Exists(filePath))
                    return NotFound(new { success = false, message = "Image not found" });

                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                var base64 = Convert.ToBase64String(fileBytes);
                var contentType = GetContentType(filePath);

                return Ok(new { 
                    success = true, 
                    data = new 
                    { 
                        base64 = base64,
                        contentType = contentType,
                        url = $"/{path}"
                    } 
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get image as Base64 by client ID
        /// GET /api/image/client/{id}
        /// </summary>
        [HttpGet("client/{id}")]
        public async Task<IActionResult> GetClientImage(int id)
        {
            try
            {
                var db = HttpContext.RequestServices.GetRequiredService<Pdd.ir.Data.IDbService>();
                var client = await db.QueryFirstOrDefaultAsync<Pdd.ir.Business.Models.Entities.Client>(
                    "SELECT ImageUrl FROM Clients WHERE Id = @Id", new { Id = id });

                if (client == null || string.IsNullOrEmpty(client.ImageUrl))
                    return NotFound(new { success = false, message = "Client or image not found" });

                var filePath = Path.Combine(_env.WebRootPath, client.ImageUrl.TrimStart('/'));
                
                if (!System.IO.File.Exists(filePath))
                    return NotFound(new { success = false, message = "Image file not found" });

                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                var base64 = Convert.ToBase64String(fileBytes);
                var contentType = GetContentType(filePath);

                return Ok(new { 
                    success = true, 
                    data = new 
                    { 
                        base64 = base64,
                        contentType = contentType,
                        url = client.ImageUrl
                    } 
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        private string GetContentType(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".svg" => "image/svg+xml",
                _ => "application/octet-stream"
            };
        }
    }
}
