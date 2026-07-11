namespace Pdd.ir.Data.Queries
{
    public static class ContactQueries
    {
        public const string GetById = "SELECT * FROM ContactMessages WHERE Id = @Id";
        public const string GetAll = "SELECT * FROM ContactMessages ORDER BY IsRead, CreatedAt DESC";
        public const string GetUnread = "SELECT * FROM ContactMessages WHERE IsRead = 0 ORDER BY CreatedAt DESC";
        public const string Insert = @"INSERT INTO ContactMessages (Name, Email, Phone, Subject, Message, IsRead, CreatedAt) 
                                       VALUES (@Name, @Email, @Phone, @Subject, @Message, @IsRead, @CreatedAt);
                                       SELECT CAST(SCOPE_IDENTITY() AS INT)";
        public const string MarkAsRead = "UPDATE ContactMessages SET IsRead = 1 WHERE Id = @Id";
        public const string Delete = "DELETE FROM ContactMessages WHERE Id = @Id";
        public const string CountAll = "SELECT COUNT(*) FROM ContactMessages";
        public const string CountUnread = "SELECT COUNT(*) FROM ContactMessages WHERE IsRead = 0";
    }
}
