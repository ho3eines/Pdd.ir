using Pdd.ir.Business.Models.DTOs;
using Pdd.ir.Business.Models.Entities;
using Pdd.ir.Data;
using Pdd.ir.Data.Queries;

namespace Pdd.ir.Business.Services
{
    public class ClientBusinessService
    {
        private readonly IDbService _db;
        private readonly string _imagePath;

        public ClientBusinessService(IDbService db, string imagePath)
        {
            _db = db;
            _imagePath = imagePath;
            // Ensure img folder exists
            Directory.CreateDirectory(_imagePath);
        }

        public async Task<IEnumerable<ClientDto>> GetAllAsync()
        {
            var clients = await _db.QueryAsync<Client>(ClientQueries.GetAll);
            return clients.Select(MapToDto);
        }

        public async Task<IEnumerable<ClientDto>> GetAllAdminAsync()
        {
            var clients = await _db.QueryAsync<Client>(ClientQueries.GetAllAdmin);
            return clients.Select(MapToDto);
        }

        public async Task<ClientDto?> GetByIdAsync(int id)
        {
            var client = await _db.QueryFirstOrDefaultAsync<Client>(ClientQueries.GetById, new { Id = id });
            return client != null ? MapToDto(client) : null;
        }

        public async Task<int> InsertAsync(ClientCreateRequest request)
        {
            string imageUrl = "";
            
            // If ImageUrl GUID is provided (from UploadController), use it directly
            if (!string.IsNullOrEmpty(request.ImageUrl))
            {
                imageUrl = request.ImageUrl;
            }
            // Otherwise if Base64 is provided, save as file
            else if (!string.IsNullOrEmpty(request.ImageBase64))
            {
                imageUrl = await SaveImageAsync(request.ImageBase64);
            }

            return await _db.ExecuteScalarAsync<int>(ClientQueries.Insert, new
            {
                request.Name,
                request.NameEn,
                ImageUrl = imageUrl,
                request.SortOrder,
                CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            });
        }

        public async Task<bool> UpdateAsync(ClientDto dto)
        {
            string imageUrl = dto.ImageUrl;
            
            System.Diagnostics.Debug.WriteLine($"[UpdateAsync] dto.ImageUrl = {(dto.ImageUrl?.Length > 50 ? dto.ImageUrl[..50] + "..." : dto.ImageUrl)}");
            
            // If ImageUrl is a data URL (starts with "data:"), save as file
            if (!string.IsNullOrEmpty(dto.ImageUrl) && dto.ImageUrl.StartsWith("data:"))
            {
                System.Diagnostics.Debug.WriteLine($"[UpdateAsync] Data URL detected, saving file...");
                imageUrl = await SaveImageAsync(dto.ImageUrl);
                System.Diagnostics.Debug.WriteLine($"[UpdateAsync] File saved: {imageUrl}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[UpdateAsync] Not a data URL, using existing: {imageUrl}");
            }

            var rows = await _db.ExecuteAsync(ClientQueries.Update, new
            {
                dto.Name,
                dto.NameEn,
                ImageUrl = imageUrl,
                dto.SortOrder,
                dto.Id
            });
            System.Diagnostics.Debug.WriteLine($"[UpdateAsync] Rows affected: {rows}");
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var rows = await _db.ExecuteAsync(ClientQueries.Delete, new { Id = id });
            return rows > 0;
        }

        private async Task<string> SaveImageAsync(string imageData)
        {
            string base64 = imageData;
            string extension = ".jpg"; // Default

            // Extract Base64 from data URL if needed
            if (imageData.StartsWith("data:"))
            {
                // Parse content type from data URL: "data:image/png;base64,..."
                if (imageData.Contains("image/png")) extension = ".png";
                else if (imageData.Contains("image/gif")) extension = ".gif";
                else if (imageData.Contains("image/webp")) extension = ".webp";
                
                // Extract Base64 part
                if (imageData.Contains(","))
                    base64 = imageData.Split(',')[1];
            }

            // Convert to byte array
            var imageBytes = Convert.FromBase64String(base64);

            // Generate GUID filename (no dashes)
            var fileName = $"{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(_imagePath, fileName);

            // Save file
            await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);

            // Return GUID only (without extension and path)
            return Path.GetFileNameWithoutExtension(fileName);
        }

        private static ClientDto MapToDto(Client c)
        {
            return new ClientDto
            {
                Id = c.Id,
                Name = c.Name,
                NameEn = c.NameEn,
                ImageUrl = c.ImageUrl,
                SortOrder = c.SortOrder,
                IsActive = c.IsActive
            };
        }
    }
}
