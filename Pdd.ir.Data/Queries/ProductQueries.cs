namespace Pdd.ir.Data.Queries
{
    public static class ProductQueries
    {
        public const string GetById = "SELECT * FROM Products WHERE Id = @Id";
        public const string GetAll = "SELECT * FROM Products WHERE IsActive = 1 ORDER BY SortOrder, Id";
        public const string GetAllAdmin = "SELECT * FROM Products ORDER BY SortOrder, Id";
        public const string GetByCategory = "SELECT * FROM Products WHERE Category = @Category AND IsActive = 1 ORDER BY SortOrder";
        public const string Insert = @"INSERT INTO Products (Title, Subtitle, Description, Features, ImageUrl, Category, SortOrder, IsActive) 
                                       VALUES (@Title, @Subtitle, @Description, @Features, @ImageUrl, @Category, @SortOrder, @IsActive);
                                       SELECT CAST(SCOPE_IDENTITY() AS INT)";
        public const string Update = @"UPDATE Products SET Title = @Title, Subtitle = @Subtitle, Description = @Description, 
                                       Features = @Features, ImageUrl = @ImageUrl, Category = @Category, SortOrder = @SortOrder, IsActive = @IsActive 
                                       WHERE Id = @Id";
        public const string Delete = "UPDATE Products SET IsActive = 0 WHERE Id = @Id";
        public const string CountAll = "SELECT COUNT(*) FROM Products WHERE IsActive = 1";
    }
}
