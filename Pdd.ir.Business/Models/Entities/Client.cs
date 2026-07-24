namespace Pdd.ir.Business.Models.Entities
{
    public class Client
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string NameEn { get; set; } = "";
        public string ImageUrl { get; set; } = "";  // e.g., "/img/abc123.jpg"
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public long CreatedAt { get; set; }
    }
}
