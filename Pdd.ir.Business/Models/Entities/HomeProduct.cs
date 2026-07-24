namespace Pdd.ir.Business.Models.Entities
{
    public class HomeProduct
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
        public long CreatedAt { get; set; }
    }
}
