using Pdd.ir.Business.Models.DTOs;
using Pdd.ir.Business.Models.Entities;
using Pdd.ir.Data;
using Pdd.ir.Data.Queries;

namespace Pdd.ir.Business.Services
{
    public class BlogBusinessService
    {
        private readonly IDbService _db;

        public BlogBusinessService(IDbService db)
        {
            _db = db;
        }

        public async Task<IEnumerable<BlogDto>> GetAllAsync()
        {
            var posts = await _db.QueryAsync<BlogPost>(BlogQueries.GetAll);
            return posts.Select(MapToDto);
        }

        public async Task<IEnumerable<BlogDto>> GetAllAdminAsync()
        {
            var posts = await _db.QueryAsync<BlogPost>(BlogQueries.GetAllAdmin);
            return posts.Select(MapToDto);
        }

        public async Task<BlogDto?> GetBySlugAsync(string slug)
        {
            var post = await _db.QueryFirstOrDefaultAsync<BlogPost>(BlogQueries.GetBySlug, new { Slug = slug });
            return post == null ? null : MapToDto(post);
        }

        public async Task<BlogDto?> GetByIdAsync(int id)
        {
            var post = await _db.QueryFirstOrDefaultAsync<BlogPost>(BlogQueries.GetById, new { Id = id });
            return post == null ? null : MapToDto(post);
        }

        public async Task<int> CreateAsync(BlogCreateRequest request)
        {
            return await _db.ExecuteScalarAsync<int>(BlogQueries.Insert, new
            {
                request.Title,
                request.Slug,
                request.Summary,
                request.Content,
                request.ImageUrl,
                request.Author,
                request.IsPublished
            });
        }

        public async Task<bool> UpdateAsync(int id, BlogCreateRequest request)
        {
            var affected = await _db.ExecuteAsync(BlogQueries.Update, new
            {
                Id = id,
                request.Title,
                request.Slug,
                request.Summary,
                request.Content,
                request.ImageUrl,
                request.Author,
                request.IsPublished
            });
            return affected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var affected = await _db.ExecuteAsync(BlogQueries.Delete, new { Id = id });
            return affected > 0;
        }

        public async Task<bool> IncrementViewCountAsync(int id)
        {
            var affected = await _db.ExecuteAsync(BlogQueries.IncrementViewCount, new { Id = id });
            return affected > 0;
        }

        public async Task<int> CountAsync()
        {
            return await _db.ExecuteScalarAsync<int>(BlogQueries.CountAll);
        }

        private static BlogDto MapToDto(BlogPost p) => new()
        {
            Id = p.Id,
            Title = p.Title,
            Slug = p.Slug,
            Summary = p.Summary,
            Content = p.Content,
            ImageUrl = p.ImageUrl,
            Author = p.Author,
            IsPublished = p.IsPublished,
            ViewCount = p.ViewCount,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        };
    }
}
