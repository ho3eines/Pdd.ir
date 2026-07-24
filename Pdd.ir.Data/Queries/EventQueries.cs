namespace Pdd.ir.Data.Queries
{
    public static class EventQueries
    {
        public const string GetAll = "SELECT * FROM Events WHERE IsActive = 1 ORDER BY EventDate DESC, SortOrder, Id";
        
        public const string GetAllAdmin = "SELECT * FROM Events ORDER BY EventDate DESC, SortOrder, Id";
        
        public const string GetById = "SELECT * FROM Events WHERE Id = @Id";
        
        public const string Insert = @"
            INSERT INTO Events (Title, TitleEn, Description, DescriptionEn, ImageUrl, Location, EventDate, EventEndDate, SortOrder, IsActive, CreatedAt) 
            VALUES (@Title, @TitleEn, @Description, @DescriptionEn, @ImageUrl, @Location, @EventDate, @EventEndDate, @SortOrder, 1, @CreatedAt);
            SELECT CAST(SCOPE_IDENTITY() AS INT)";
        
        public const string Update = @"
            UPDATE Events 
            SET Title = @Title, TitleEn = @TitleEn, Description = @Description, DescriptionEn = @DescriptionEn, 
                ImageUrl = @ImageUrl, Location = @Location, EventDate = @EventDate, EventEndDate = @EventEndDate, SortOrder = @SortOrder
            WHERE Id = @Id";
        
        public const string Delete = "DELETE FROM Events WHERE Id = @Id";
    }
}
