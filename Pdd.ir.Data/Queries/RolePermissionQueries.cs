namespace Pdd.ir.Data.Queries
{
    public static class RoleQueries
    {
        public const string GetAll = "SELECT * FROM Roles ORDER BY Id";
        public const string GetById = "SELECT * FROM Roles WHERE Id = @Id";
        public const string GetByName = "SELECT * FROM Roles WHERE Name = @Name";
        public const string Insert = @"INSERT INTO Roles (Name, Description) VALUES (@Name, @Description);
                                       SELECT CAST(SCOPE_IDENTITY() AS INT)";
        public const string Update = "UPDATE Roles SET Name = @Name, Description = @Description WHERE Id = @Id";
        public const string Delete = "DELETE FROM Roles WHERE Id = @Id";
        public const string CountUsers = "SELECT COUNT(*) FROM Users WHERE Role = @RoleName";
    }

    public static class PermissionQueries
    {
        public const string GetAll = "SELECT * FROM Permissions ORDER BY Category, Name";
        public const string GetById = "SELECT * FROM Permissions WHERE Id = @Id";
        public const string GetByRoleId = @"SELECT p.* FROM Permissions p
                                             INNER JOIN RolePermissions rp ON p.Id = rp.PermissionId
                                             WHERE rp.RoleId = @RoleId ORDER BY p.Category, p.Name";
        public const string Insert = @"INSERT INTO Permissions (Name, Label, Category) VALUES (@Name, @Label, @Category);
                                       SELECT CAST(SCOPE_IDENTITY() AS INT)";
        public const string Update = "UPDATE Permissions SET Name = @Name, Label = @Label, Category = @Category WHERE Id = @Id";
        public const string Delete = "DELETE FROM Permissions WHERE Id = @Id";
    }

    public static class RolePermissionQueries
    {
        public const string GetByRoleId = "SELECT PermissionId FROM RolePermissions WHERE RoleId = @RoleId";
        public const string DeleteByRoleId = "DELETE FROM RolePermissions WHERE RoleId = @RoleId";
        public const string Insert = "INSERT INTO RolePermissions (RoleId, PermissionId) VALUES (@RoleId, @PermissionId)";
    }
}
