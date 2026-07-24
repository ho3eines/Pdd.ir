using Pdd.ir.Business.Models.DTOs;
using Pdd.ir.Business.Models.Entities;
using Pdd.ir.Data;
using Pdd.ir.Data.Queries;

namespace Pdd.ir.Business.Services
{
    public class EventBusinessService
    {
        private readonly IDbService _db;
        private readonly string _imagePath;

        public EventBusinessService(IDbService db, string imagePath)
        {
            _db = db;
            _imagePath = imagePath;
            Directory.CreateDirectory(_imagePath);
        }

        public async Task<IEnumerable<EventDto>> GetAllAsync()
        {
            var items = await _db.QueryAsync<Event>(EventQueries.GetAll);
            return items.Select(MapToDto);
        }

        public async Task<EventDto?> GetByIdAsync(int id)
        {
            var item = await _db.QueryFirstOrDefaultAsync<Event>(EventQueries.GetById, new { Id = id });
            return item != null ? MapToDto(item) : null;
        }

        public async Task<int> InsertAsync(EventCreateRequest request)
        {
            string imageUrl = "";
            if (!string.IsNullOrEmpty(request.ImageBase64))
            {
                imageUrl = await SaveImageAsync(request.ImageBase64);
            }

            return await _db.ExecuteScalarAsync<int>(EventQueries.Insert, new
            {
                request.Title,
                request.TitleEn,
                request.Description,
                request.DescriptionEn,
                ImageUrl = imageUrl,
                request.Location,
                request.EventDate,
                request.EventEndDate,
                request.SortOrder,
                CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            });
        }

        public async Task<bool> UpdateAsync(EventDto dto)
        {
            string imageUrl = dto.ImageUrl;
            if (!string.IsNullOrEmpty(dto.ImageUrl) && dto.ImageUrl.StartsWith("data:"))
            {
                imageUrl = await SaveImageAsync(dto.ImageUrl);
            }

            var rows = await _db.ExecuteAsync(EventQueries.Update, new
            {
                dto.Title,
                dto.TitleEn,
                dto.Description,
                dto.DescriptionEn,
                ImageUrl = imageUrl,
                dto.Location,
                dto.EventDate,
                dto.EventEndDate,
                dto.SortOrder,
                dto.Id
            });
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var rows = await _db.ExecuteAsync(EventQueries.Delete, new { Id = id });
            return rows > 0;
        }

        private async Task<string> SaveImageAsync(string imageData)
        {
            string base64 = imageData;
            string extension = ".jpg";

            if (imageData.StartsWith("data:"))
            {
                if (imageData.Contains("image/png")) extension = ".png";
                else if (imageData.Contains("image/gif")) extension = ".gif";
                else if (imageData.Contains("image/webp")) extension = ".webp";
                if (imageData.Contains(",")) base64 = imageData.Split(',')[1];
            }

            var imageBytes = Convert.FromBase64String(base64);
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(_imagePath, fileName);
            await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);
            return Path.GetFileNameWithoutExtension(fileName);
        }

        private static EventDto MapToDto(Event e)
        {
            return new EventDto
            {
                Id = e.Id,
                Title = e.Title,
                TitleEn = e.TitleEn,
                Description = e.Description,
                DescriptionEn = e.DescriptionEn,
                ImageUrl = e.ImageUrl,
                Location = e.Location,
                EventDate = e.EventDate,
                EventEndDate = e.EventEndDate,
                SortOrder = e.SortOrder,
                IsActive = e.IsActive
            };
        }
    }
}
