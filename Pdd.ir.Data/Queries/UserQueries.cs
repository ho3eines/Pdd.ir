namespace Pdd.ir.Data.Queries
{
    public static class UserQueries
    {
        public const string GetById = "SELECT * FROM Users WHERE Id = @Id";
        public const string GetByUsername = "SELECT * FROM Users WHERE Username = @Username AND IsActive = 1";
        public const string GetAll = "SELECT * FROM Users WHERE IsActive = 1 ORDER BY CreatedAt DESC";
        public const string GetAllAdmin = "SELECT * FROM Users ORDER BY CreatedAt DESC";
        public const string Insert = @"INSERT INTO Users (Username, PasswordHash, Email, FullName, Role, IsActive, CreatedAt)
                                       VALUES (@Username, @PasswordHash, @Email, @FullName, @Role, @IsActive, @CreatedAt);
                                       SELECT CAST(SCOPE_IDENTITY() AS INT)";
        public const string Update = @"UPDATE Users SET Email = @Email, FullName = @FullName, Role = @Role WHERE Id = @Id";
        public const string UpdatePassword = "UPDATE Users SET PasswordHash = @PasswordHash WHERE Id = @Id";
        public const string SetActive = "UPDATE Users SET IsActive = @IsActive WHERE Id = @Id";
        public const string Delete = "UPDATE Users SET IsActive = 0 WHERE Id = @Id";
        public const string CountAll = "SELECT COUNT(*) FROM Users WHERE IsActive = 1";

        public const string InsertRefreshToken = @"INSERT INTO RefreshTokens (UserId, Token, ExpiresAt, CreatedAt, IsRevoked)
                                                    VALUES (@UserId, @Token, @ExpiresAt, @CreatedAt, @IsRevoked)";
        public const string GetRefreshToken = "SELECT * FROM RefreshTokens WHERE Token = @Token AND IsRevoked = 0 AND ExpiresAt > GETUTCDATE()";
        public const string RevokeRefreshToken = "UPDATE RefreshTokens SET IsRevoked = 1 WHERE Token = @Token";
        public const string RevokeAllRefreshTokens = "UPDATE RefreshTokens SET IsRevoked = 1 WHERE UserId = @UserId";
    }
}
