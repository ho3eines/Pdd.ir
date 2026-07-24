namespace Pdd.ir.Data.Queries
{
    public static class HomeProductQueries
    {
        public const string GetAll = "SELECT * FROM HomeProducts WHERE IsActive = 1 ORDER BY SortOrder";
        public const string GetAllAdmin = "SELECT * FROM HomeProducts ORDER BY SortOrder";
        public const string GetById = "SELECT * FROM HomeProducts WHERE Id = @Id";
        public const string Insert = @"INSERT INTO HomeProducts (NameFa, NameEn, SubFa, SubEn, Icon, IconBg, IconColor, Route, SortOrder, IsActive, CreatedAt) 
            VALUES (@NameFa, @NameEn, @SubFa, @SubEn, @Icon, @IconBg, @IconColor, @Route, @SortOrder, 1, @CreatedAt); 
            SELECT CAST(SCOPE_IDENTITY() AS INT)";
        public const string Update = @"UPDATE HomeProducts SET 
            NameFa = @NameFa, NameEn = @NameEn, SubFa = @SubFa, SubEn = @SubEn,
            Icon = @Icon, IconBg = @IconBg, IconColor = @IconColor, Route = @Route,
            SortOrder = @SortOrder, IsActive = @IsActive 
            WHERE Id = @Id";
        public const string Delete = "DELETE FROM HomeProducts WHERE Id = @Id";
    }
}
