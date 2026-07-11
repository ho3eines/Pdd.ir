using Pdd.ir.Business.Models.DTOs;
using Pdd.ir.Business.Models.Entities;
using Pdd.ir.Data;
using Pdd.ir.Data.Queries;

namespace Pdd.ir.Business.Services
{
    public class ProductBusinessService
    {
        private readonly IDbService _db;

        public ProductBusinessService(IDbService db)
        {
            _db = db;
        }

        public async Task<IEnumerable<ProductDto>> GetAllAsync()
        {
            var products = await _db.QueryAsync<Product>(ProductQueries.GetAll);
            return products.Select(MapToDto);
        }

        public async Task<IEnumerable<ProductDto>> GetByCategoryAsync(string category)
        {
            var products = await _db.QueryAsync<Product>(ProductQueries.GetByCategory, new { Category = category });
            return products.Select(MapToDto);
        }

        public async Task<ProductDto?> GetByIdAsync(int id)
        {
            var product = await _db.QueryFirstOrDefaultAsync<Product>(ProductQueries.GetById, new { Id = id });
            return product == null ? null : MapToDto(product);
        }

        public async Task<int> CreateAsync(ProductCreateRequest request)
        {
            return await _db.ExecuteScalarAsync<int>(ProductQueries.Insert, new
            {
                request.Title,
                request.Subtitle,
                request.Description,
                request.Features,
                request.ImageUrl,
                request.Category,
                request.SortOrder,
                IsActive = true
            });
        }

        public async Task<bool> UpdateAsync(int id, ProductCreateRequest request)
        {
            var affected = await _db.ExecuteAsync(ProductQueries.Update, new
            {
                Id = id,
                request.Title,
                request.Subtitle,
                request.Description,
                request.Features,
                request.ImageUrl,
                request.Category,
                request.SortOrder,
                IsActive = true
            });
            return affected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var affected = await _db.ExecuteAsync(ProductQueries.Delete, new { Id = id });
            return affected > 0;
        }

        public async Task<int> CountAsync()
        {
            return await _db.ExecuteScalarAsync<int>(ProductQueries.CountAll);
        }

        private static ProductDto MapToDto(Product p) => new()
        {
            Id = p.Id,
            Title = p.Title,
            Subtitle = p.Subtitle,
            Description = p.Description,
            Features = p.Features,
            ImageUrl = p.ImageUrl,
            Category = p.Category,
            SortOrder = p.SortOrder,
            IsActive = p.IsActive
        };
    }
}
