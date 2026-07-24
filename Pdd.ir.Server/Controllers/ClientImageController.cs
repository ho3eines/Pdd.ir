using Microsoft.AspNetCore.Mvc;

namespace Pdd.ir.Server.Controllers
{
    [ApiController]
    [Route("api/client-image")]
    public class ClientImageController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public ClientImageController(IWebHostEnvironment env)
        {
            _env = env;
        }

        /// <summary>
        /// Get all client images as Base64
        /// GET /api/client-image/all
        /// </summary>
        [HttpGet("all")]
        public async Task<IActionResult> GetAllClientImages()
        {
            try
            {
                var db = HttpContext.RequestServices.GetRequiredService<Pdd.ir.Data.IDbService>();
                var clients = await db.QueryAsync<Pdd.ir.Business.Models.Entities.Client>(
                    "SELECT Id, Name, NameEn, ImageUrl FROM Clients WHERE IsActive = 1 AND ImageUrl != '' ORDER BY SortOrder");

                var result = clients.Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.NameEn,
                    ImageUrl = GetImageUrl(c.ImageUrl)
                }).ToList();

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get client image by ID as Base64
        /// GET /api/client-image/{id}
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetClientImage(int id)
        {
            try
            {
                var db = HttpContext.RequestServices.GetRequiredService<Pdd.ir.Data.IDbService>();
                var client = await db.QueryFirstOrDefaultAsync<Pdd.ir.Business.Models.Entities.Client>(
                    "SELECT Id, Name, NameEn, ImageUrl FROM Clients WHERE Id = @Id", new { Id = id });

                if (client == null)
                    return NotFound(new { success = false, message = "Client not found" });

                if (string.IsNullOrEmpty(client.ImageUrl))
                    return Ok(new { success = true, data = new { client.Id, client.Name, client.NameEn, ImageUrl = (string?)null } });

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        client.Id,
                        client.Name,
                        client.NameEn,
                        ImageUrl = GetImageUrl(client.ImageUrl)
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        private string? GetImageUrl(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return null;

            var filePath = Path.Combine(_env.WebRootPath, imageUrl.TrimStart('/'));
            
            if (!System.IO.File.Exists(filePath))
                return null;

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            var base64 = Convert.ToBase64String(fileBytes);
            var contentType = GetContentType(filePath);
            
            return $"data:{contentType};base64,{base64}";
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
