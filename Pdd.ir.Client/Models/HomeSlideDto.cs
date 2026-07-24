namespace Pdd.ir.Client.Models
{
    public class HomeSlideDto
    {
        public int Id { get; set; }
        public string BadgeFa { get; set; } = "";
        public string BadgeEn { get; set; } = "";
        public string TitleFa { get; set; } = "";
        public string TitleEn { get; set; } = "";
        public string DescFa { get; set; } = "";
        public string DescEn { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
    }
}
