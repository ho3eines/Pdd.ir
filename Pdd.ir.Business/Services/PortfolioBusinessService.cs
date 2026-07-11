using Pdd.ir.Business.Models.DTOs;
using Pdd.ir.Business.Models.Entities;
using Pdd.ir.Data;
using Pdd.ir.Data.Queries;

namespace Pdd.ir.Business.Services
{
    public class PortfolioBusinessService
    {
        private readonly IDbService _db;

        public PortfolioBusinessService(IDbService db)
        {
            _db = db;
        }

        public async Task<IEnumerable<PortfolioDto>> GetAllAsync()
        {
            var items = await _db.QueryAsync<PortfolioItem>(PortfolioQueries.GetAll);
            return items.Select(MapToDto);
        }

        public async Task<IEnumerable<PortfolioDto>> GetAllAdminAsync()
        {
            var items = await _db.QueryAsync<PortfolioItem>(PortfolioQueries.GetAllAdmin);
            return items.Select(MapToDto);
        }

        public async Task<PortfolioDto?> GetByIdAsync(int id)
        {
            var item = await _db.QueryFirstOrDefaultAsync<PortfolioItem>(PortfolioQueries.GetById, new { Id = id });
            return item == null ? null : MapToDto(item);
        }

        public async Task<int> CreateAsync(PortfolioCreateRequest request)
        {
            return await _db.ExecuteScalarAsync<int>(PortfolioQueries.Insert, new
            {
                request.Title,
                request.Description,
                request.ImageUrl,
                request.ProjectUrl,
                request.Category,
                request.SortOrder,
                request.IsActive
            });
        }

        public async Task<bool> UpdateAsync(int id, PortfolioCreateRequest request)
        {
            var affected = await _db.ExecuteAsync(PortfolioQueries.Update, new
            {
                Id = id,
                request.Title,
                request.Description,
                request.ImageUrl,
                request.ProjectUrl,
                request.Category,
                request.SortOrder,
                request.IsActive
            });
            return affected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var affected = await _db.ExecuteAsync(PortfolioQueries.Delete, new { Id = id });
            return affected > 0;
        }

        public async Task<int> CountAsync()
        {
            return await _db.ExecuteScalarAsync<int>(PortfolioQueries.CountAll);
        }

        private static PortfolioDto MapToDto(PortfolioItem p) => new()
        {
            Id = p.Id,
            Title = p.Title,
            Description = p.Description,
            ImageUrl = p.ImageUrl,
            ProjectUrl = p.ProjectUrl,
            Category = p.Category,
            SortOrder = p.SortOrder,
            IsActive = p.IsActive,
            CreatedAt = p.CreatedAt
        };
    }
}
