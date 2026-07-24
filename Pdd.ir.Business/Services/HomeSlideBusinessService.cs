using Pdd.ir.Business.Models.DTOs;
using Pdd.ir.Business.Models.Entities;
using Pdd.ir.Data;
using Pdd.ir.Data.Queries;

namespace Pdd.ir.Business.Services
{
    public class HomeSlideBusinessService
    {
        private readonly IDbService _db;

        public HomeSlideBusinessService(IDbService db) { _db = db; }

        public async Task<IEnumerable<HomeSlideDto>> GetAllAsync()
        {
            var items = await _db.QueryAsync<HomeSlide>(HomeSlideQueries.GetAll);
            return items.Select(MapToDto);
        }

        public async Task<IEnumerable<HomeSlideDto>> GetAllAdminAsync()
        {
            var items = await _db.QueryAsync<HomeSlide>(HomeSlideQueries.GetAllAdmin);
            return items.Select(MapToDto);
        }

        public async Task<HomeSlideDto?> GetByIdAsync(int id)
        {
            var item = await _db.QueryFirstOrDefaultAsync<HomeSlide>(HomeSlideQueries.GetById, new { Id = id });
            return item != null ? MapToDto(item) : null;
        }

        public async Task<int> InsertAsync(HomeSlideCreateRequest request)
        {
            return await _db.ExecuteScalarAsync<int>(HomeSlideQueries.Insert, new
            {
                request.BadgeFa,
                request.BadgeEn,
                request.TitleFa,
                request.TitleEn,
                request.DescFa,
                request.DescEn,
                ImageUrl = request.ImageUrl ?? "",
                request.SortOrder,
                CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            });
        }

        public async Task<bool> UpdateAsync(HomeSlideDto dto)
        {
            var rows = await _db.ExecuteAsync(HomeSlideQueries.Update, new
            {
                dto.BadgeFa,
                dto.BadgeEn,
                dto.TitleFa,
                dto.TitleEn,
                dto.DescFa,
                dto.DescEn,
                dto.ImageUrl,
                dto.SortOrder,
                dto.IsActive,
                dto.Id
            });
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var rows = await _db.ExecuteAsync(HomeSlideQueries.Delete, new { Id = id });
            return rows > 0;
        }

        private static HomeSlideDto MapToDto(HomeSlide s)
        {
            return new HomeSlideDto
            {
                Id = s.Id,
                BadgeFa = s.BadgeFa,
                BadgeEn = s.BadgeEn,
                TitleFa = s.TitleFa,
                TitleEn = s.TitleEn,
                DescFa = s.DescFa,
                DescEn = s.DescEn,
                ImageUrl = s.ImageUrl,
                SortOrder = s.SortOrder,
                IsActive = s.IsActive
            };
        }
    }
}
