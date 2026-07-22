namespace Pdd.ir.Business.Models.DTOs
{
    public class ClientDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string NameEn { get; set; } = "";
        public string Icon { get; set; } = "bi-hospital";
        public string Color { get; set; } = "#0D6EFD";
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
    }

    public class ClientCreateRequest
    {
        public string Name { get; set; } = "";
        public string NameEn { get; set; } = "";
        public string Icon { get; set; } = "bi-hospital";
        public string Color { get; set; } = "#0D6EFD";
        public int SortOrder { get; set; }
    }
}
