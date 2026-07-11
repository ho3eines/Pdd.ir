namespace Pdd.ir.Data.Queries
{
    public static class PageQueries
    {
        public const string GetById = "SELECT * FROM Pages WHERE Id = @Id";
        public const string GetBySlug = "SELECT * FROM Pages WHERE Slug = @Slug AND IsActive = 1";
        public const string GetAll = "SELECT * FROM Pages WHERE IsActive = 1 ORDER BY Id";
        public const string GetAllAdmin = "SELECT * FROM Pages ORDER BY Id";
        public const string Insert = @"INSERT INTO Pages (Slug, Title, Content, MetaDescription, IsActive, UpdatedAt) 
                                       VALUES (@Slug, @Title, @Content, @MetaDescription, @IsActive, @UpdatedAt);
                                       SELECT CAST(SCOPE_IDENTITY() AS INT)";
        public const string Update = @"UPDATE Pages SET Slug = @Slug, Title = @Title, Content = @Content, 
                                       MetaDescription = @MetaDescription, IsActive = @IsActive, UpdatedAt = @UpdatedAt WHERE Id = @Id";
        public const string Delete = "UPDATE Pages SET IsActive = 0 WHERE Id = @Id";
    }
}
