namespace Pdd.ir.Data.Queries
{
    public static class ClientQueries
    {
        public const string GetAll = "SELECT * FROM Clients WHERE IsActive = 1 ORDER BY SortOrder, Id";
        
        public const string GetAllAdmin = "SELECT * FROM Clients ORDER BY SortOrder, Id";
        
        public const string GetById = "SELECT * FROM Clients WHERE Id = @Id";
        
        public const string Insert = @"
            INSERT INTO Clients (Name, NameEn, Icon, Color, SortOrder, IsActive, CreatedAt) 
            VALUES (@Name, @NameEn, @Icon, @Color, @SortOrder, 1, @CreatedAt);
            SELECT CAST(SCOPE_IDENTITY() AS INT)";
        
        public const string Update = @"
            UPDATE Clients 
            SET Name = @Name, NameEn = @NameEn, Icon = @Icon, Color = @Color, SortOrder = @SortOrder 
            WHERE Id = @Id";
        
        public const string SoftDelete = "UPDATE Clients SET IsActive = 0 WHERE Id = @Id";
        
        public const string SetActive = "UPDATE Clients SET IsActive = @IsActive WHERE Id = @Id";
    }
}
