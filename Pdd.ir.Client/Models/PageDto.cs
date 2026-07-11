namespace Pdd.ir.Client.Models
{
    public class PageDto
    {
        public int Id { get; set; }
        public string Slug { get; set; } = "";
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
        public string MetaDescription { get; set; } = "";
        public bool IsActive { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class PageUpdateRequest
    {
        public int Id { get; set; }
        public string Slug { get; set; } = "";
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
        public string MetaDescription { get; set; } = "";
        public bool IsActive { get; set; } = true;
    }
}
