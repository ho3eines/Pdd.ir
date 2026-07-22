namespace Pdd.ir.Business.Models.Entities
{
    public class Client
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string NameEn { get; set; } = "";
        public string Icon { get; set; } = "bi-hospital";
        public string Color { get; set; } = "#0D6EFD";
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public long CreatedAt { get; set; }
    }
}
