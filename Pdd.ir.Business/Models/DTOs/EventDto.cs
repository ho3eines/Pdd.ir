namespace Pdd.ir.Business.Models.DTOs
{
    public class EventDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string TitleEn { get; set; } = "";
        public string Description { get; set; } = "";
        public string DescriptionEn { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public string Location { get; set; } = "";
        public long EventDate { get; set; }
        public long EventEndDate { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
    }

    public class EventCreateRequest
    {
        public string Title { get; set; } = "";
        public string TitleEn { get; set; } = "";
        public string Description { get; set; } = "";
        public string DescriptionEn { get; set; } = "";
        public string? ImageBase64 { get; set; }
        public string Location { get; set; } = "";
        public long EventDate { get; set; }
        public long EventEndDate { get; set; }
        public int SortOrder { get; set; }
    }
}
