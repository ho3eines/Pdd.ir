namespace Pdd.ir.Client.Models
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Subtitle { get; set; } = "";
        public string Description { get; set; } = "";
        public string Features { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public string Category { get; set; } = "";
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
    }

    public class ProductCreateRequest
    {
        public string Title { get; set; } = "";
        public string Subtitle { get; set; } = "";
        public string Description { get; set; } = "";
        public string Features { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public string Category { get; set; } = "";
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
