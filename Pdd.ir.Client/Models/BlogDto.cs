namespace Pdd.ir.Client.Models
{
    public class BlogDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Slug { get; set; } = "";
        public string Summary { get; set; } = "";
        public string Content { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public string Author { get; set; } = "";
        public bool IsPublished { get; set; }
        public int ViewCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class BlogCreateRequest
    {
        public string Title { get; set; } = "";
        public string Slug { get; set; } = "";
        public string Summary { get; set; } = "";
        public string Content { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public string Author { get; set; } = "";
        public bool IsPublished { get; set; }
    }
}
