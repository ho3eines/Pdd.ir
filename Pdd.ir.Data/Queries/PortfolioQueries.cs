namespace Pdd.ir.Data.Queries
{
    public static class PortfolioQueries
    {
        public const string GetById = "SELECT * FROM PortfolioItems WHERE Id = @Id";
        public const string GetAll = "SELECT * FROM PortfolioItems WHERE IsActive = 1 ORDER BY SortOrder, Id";
        public const string GetAllAdmin = "SELECT * FROM PortfolioItems ORDER BY SortOrder, Id";
        public const string Insert = @"INSERT INTO PortfolioItems (Title, Description, ImageUrl, ProjectUrl, Category, SortOrder, IsActive, CreatedAt) 
                                       VALUES (@Title, @Description, @ImageUrl, @ProjectUrl, @Category, @SortOrder, @IsActive, GETUTCDATE());
                                       SELECT CAST(SCOPE_IDENTITY() AS INT)";
        public const string Update = @"UPDATE PortfolioItems SET Title = @Title, Description = @Description, ImageUrl = @ImageUrl, 
                                       ProjectUrl = @ProjectUrl, Category = @Category, SortOrder = @SortOrder, IsActive = @IsActive 
                                       WHERE Id = @Id";
        public const string Delete = "DELETE FROM PortfolioItems WHERE Id = @Id";
        public const string CountAll = "SELECT COUNT(*) FROM PortfolioItems WHERE IsActive = 1";
    }
}
