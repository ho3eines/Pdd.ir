namespace Pdd.ir.Client.Models
{
    public class PortfolioDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public string ProjectUrl { get; set; } = "";
        public string Category { get; set; } = "";
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PortfolioCreateRequest
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public string ProjectUrl { get; set; } = "";
        public string Category { get; set; } = "";
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
