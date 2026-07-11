namespace Pdd.ir.Client.Models
{
    public class ContactDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Subject { get; set; } = "";
        public string Message { get; set; } = "";
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ContactRequest
    {
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Subject { get; set; } = "";
        public string Message { get; set; } = "";
    }
}
