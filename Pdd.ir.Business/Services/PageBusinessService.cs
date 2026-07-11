using Pdd.ir.Business.Models.DTOs;
using Pdd.ir.Business.Models.Entities;
using Pdd.ir.Data;
using Pdd.ir.Data.Queries;

namespace Pdd.ir.Business.Services
{
    public class PageBusinessService
    {
        private readonly IDbService _db;

        public PageBusinessService(IDbService db)
        {
            _db = db;
        }

        public async Task<PageDto?> GetBySlugAsync(string slug)
        {
            var page = await _db.QueryFirstOrDefaultAsync<Page>(PageQueries.GetBySlug, new { Slug = slug });
            return page == null ? null : MapToDto(page);
        }

        public async Task<PageDto?> GetByIdAsync(int id)
        {
            var page = await _db.QueryFirstOrDefaultAsync<Page>(PageQueries.GetById, new { Id = id });
            return page == null ? null : MapToDto(page);
        }

        public async Task<IEnumerable<PageDto>> GetAllAsync()
        {
            var pages = await _db.QueryAsync<Page>(PageQueries.GetAll);
            return pages.Select(MapToDto);
        }

        public async Task<int> CreateAsync(PageUpdateRequest request)
        {
            return await _db.ExecuteScalarAsync<int>(PageQueries.Insert, new
            {
                request.Slug,
                request.Title,
                request.Content,
                request.MetaDescription,
                IsActive = true,
                UpdatedAt = DateTime.UtcNow
            });
        }

        public async Task<bool> UpdateAsync(PageUpdateRequest request)
        {
            var affected = await _db.ExecuteAsync(PageQueries.Update, new
            {
                request.Id,
                request.Slug,
                request.Title,
                request.Content,
                request.MetaDescription,
                IsActive = true,
                UpdatedAt = DateTime.UtcNow
            });
            return affected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var affected = await _db.ExecuteAsync(PageQueries.Delete, new { Id = id });
            return affected > 0;
        }

        private static PageDto MapToDto(Page p) => new()
        {
            Id = p.Id,
            Slug = p.Slug,
            Title = p.Title,
            Content = p.Content,
            MetaDescription = p.MetaDescription,
            IsActive = p.IsActive
        };
    }
}
