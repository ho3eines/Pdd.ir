using Pdd.ir.Business.Models.DTOs;
using Pdd.ir.Business.Models.Entities;
using Pdd.ir.Data;
using Pdd.ir.Data.Queries;

namespace Pdd.ir.Business.Services
{
    public class HomeProductBusinessService
    {
        private readonly IDbService _db;

        public HomeProductBusinessService(IDbService db) { _db = db; }

        public async Task<IEnumerable<HomeProductDto>> GetAllAsync()
        {
            var items = await _db.QueryAsync<HomeProduct>(HomeProductQueries.GetAll);
            return items.Select(MapToDto);
        }

        public async Task<IEnumerable<HomeProductDto>> GetAllAdminAsync()
        {
            var items = await _db.QueryAsync<HomeProduct>(HomeProductQueries.GetAllAdmin);
            return items.Select(MapToDto);
        }

        public async Task<HomeProductDto?> GetByIdAsync(int id)
        {
            var item = await _db.QueryFirstOrDefaultAsync<HomeProduct>(HomeProductQueries.GetById, new { Id = id });
            return item != null ? MapToDto(item) : null;
        }

        public async Task<int> InsertAsync(HomeProductCreateRequest request)
        {
            return await _db.ExecuteScalarAsync<int>(HomeProductQueries.Insert, new
            {
                request.NameFa,
                request.NameEn,
                request.SubFa,
                request.SubEn,
                request.Icon,
                request.IconBg,
                request.IconColor,
                request.Route,
                request.SortOrder,
                CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            });
        }

        public async Task<bool> UpdateAsync(HomeProductDto dto)
        {
            var rows = await _db.ExecuteAsync(HomeProductQueries.Update, new
            {
                dto.NameFa,
                dto.NameEn,
                dto.SubFa,
                dto.SubEn,
                dto.Icon,
                dto.IconBg,
                dto.IconColor,
                dto.Route,
                dto.SortOrder,
                dto.IsActive,
                dto.Id
            });
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var rows = await _db.ExecuteAsync(HomeProductQueries.Delete, new { Id = id });
            return rows > 0;
        }

        private static HomeProductDto MapToDto(HomeProduct p)
        {
            return new HomeProductDto
            {
                Id = p.Id,
                NameFa = p.NameFa,
                NameEn = p.NameEn,
                SubFa = p.SubFa,
                SubEn = p.SubEn,
                Icon = p.Icon,
                IconBg = p.IconBg,
                IconColor = p.IconColor,
                Route = p.Route,
                SortOrder = p.SortOrder,
                IsActive = p.IsActive
            };
        }
    }
}
