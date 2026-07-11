namespace Pdd.ir.Data.Queries
{
    public static class BlogQueries
    {
        public const string GetById = "SELECT * FROM BlogPosts WHERE Id = @Id";
        public const string GetBySlug = "SELECT * FROM BlogPosts WHERE Slug = @Slug";
        public const string GetAll = "SELECT * FROM BlogPosts WHERE IsPublished = 1 ORDER BY CreatedAt DESC";
        public const string GetAllAdmin = "SELECT * FROM BlogPosts ORDER BY CreatedAt DESC";
        public const string Insert = @"INSERT INTO BlogPosts (Title, Slug, Summary, Content, ImageUrl, Author, IsPublished, ViewCount, CreatedAt, UpdatedAt) 
                                       VALUES (@Title, @Slug, @Summary, @Content, @ImageUrl, @Author, @IsPublished, 0, GETUTCDATE(), GETUTCDATE());
                                       SELECT CAST(SCOPE_IDENTITY() AS INT)";
        public const string Update = @"UPDATE BlogPosts SET Title = @Title, Slug = @Slug, Summary = @Summary, Content = @Content, 
                                       ImageUrl = @ImageUrl, Author = @Author, IsPublished = @IsPublished, UpdatedAt = GETUTCDATE() 
                                       WHERE Id = @Id";
        public const string Delete = "DELETE FROM BlogPosts WHERE Id = @Id";
        public const string CountAll = "SELECT COUNT(*) FROM BlogPosts WHERE IsPublished = 1";
        public const string IncrementViewCount = "UPDATE BlogPosts SET ViewCount = ViewCount + 1 WHERE Id = @Id";
    }
}
