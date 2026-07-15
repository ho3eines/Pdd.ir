namespace Pdd.ir.Client.Models
{
    public class RoleDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
    }

    public class RoleDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public List<int> PermissionIds { get; set; } = new();
    }

    public class PermissionDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Label { get; set; } = "";
        public string Category { get; set; } = "";
    }

    public class RoleCreateRequest
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public List<int> PermissionIds { get; set; } = new();
    }
}
