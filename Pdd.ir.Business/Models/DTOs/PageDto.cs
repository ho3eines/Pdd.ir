namespace Pdd.ir.Business.Models.DTOs
{
    public class PageDto
    {
        public int Id { get; set; }
        public string Slug { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string MetaDescription { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class PageUpdateRequest
    {
        public int Id { get; set; }
        public string Slug { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string MetaDescription { get; set; } = string.Empty;
    }
}
