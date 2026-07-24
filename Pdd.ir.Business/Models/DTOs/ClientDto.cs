namespace Pdd.ir.Business.Models.DTOs
{
    public class ClientDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string NameEn { get; set; } = "";
        public string ImageUrl { get; set; } = "";  // File path like "/img/abc123.jpg"
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
    }

    public class ClientCreateRequest
    {
        public string Name { get; set; } = "";
        public string NameEn { get; set; } = "";
        public string? ImageBase64 { get; set; }  // Base64 from client
        public int SortOrder { get; set; }
    }
}
