namespace Pdd.ir.Data.Queries
{
    public static class HomeSlideQueries
    {
        public const string GetAll = "SELECT * FROM HomeSlides WHERE IsActive = 1 ORDER BY SortOrder";
        public const string GetAllAdmin = "SELECT * FROM HomeSlides ORDER BY SortOrder";
        public const string GetById = "SELECT * FROM HomeSlides WHERE Id = @Id";
        public const string Insert = @"INSERT INTO HomeSlides (BadgeFa, BadgeEn, TitleFa, TitleEn, DescFa, DescEn, ImageUrl, SortOrder, IsActive, CreatedAt) 
            VALUES (@BadgeFa, @BadgeEn, @TitleFa, @TitleEn, @DescFa, @DescEn, @ImageUrl, @SortOrder, 1, @CreatedAt); 
            SELECT CAST(SCOPE_IDENTITY() AS INT)";
        public const string Update = @"UPDATE HomeSlides SET 
            BadgeFa = @BadgeFa, BadgeEn = @BadgeEn, TitleFa = @TitleFa, TitleEn = @TitleEn,
            DescFa = @DescFa, DescEn = @DescEn, ImageUrl = @ImageUrl, SortOrder = @SortOrder, IsActive = @IsActive 
            WHERE Id = @Id";
        public const string Delete = "DELETE FROM HomeSlides WHERE Id = @Id";
    }
}
