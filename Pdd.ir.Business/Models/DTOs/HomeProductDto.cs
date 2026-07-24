namespace Pdd.ir.Business.Models.DTOs
{
    public class HomeProductDto
    {
        public int Id { get; set; }
        public string NameFa { get; set; } = "";
        public string NameEn { get; set; } = "";
        public string SubFa { get; set; } = "";
        public string SubEn { get; set; } = "";
        public string Icon { get; set; } = "";
        public string IconBg { get; set; } = "";
        public string IconColor { get; set; } = "";
        public string Route { get; set; } = "";
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
    }

    public class HomeProductCreateRequest
    {
        public string NameFa { get; set; } = "";
        public string NameEn { get; set; } = "";
        public string SubFa { get; set; } = "";
        public string SubEn { get; set; } = "";
        public string Icon { get; set; } = "bi-box";
        public string IconBg { get; set; } = "rgba(13,110,253,0.15)";
        public string IconColor { get; set; } = "#0D6EFD";
        public string Route { get; set; } = "/products";
        public int SortOrder { get; set; }
    }
}
