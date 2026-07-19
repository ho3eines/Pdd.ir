namespace Pdd.ir.Server.Models
{
    public class AuthSession
    {
        public int Id { get; set; }
        public string ClientId { get; set; } = "";
        public string TokenHash { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsActive { get; set; }
    }
}
