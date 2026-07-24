namespace Pdd.ir.Business.Models.Entities
{
    public class Event
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string TitleEn { get; set; } = "";
        public string Description { get; set; } = "";
        public string DescriptionEn { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public string Location { get; set; } = "";
        public long EventDate { get; set; }        // Unix timestamp (Shamsi)
        public long EventEndDate { get; set; }      // Unix timestamp (optional)
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public long CreatedAt { get; set; }
    }
}
