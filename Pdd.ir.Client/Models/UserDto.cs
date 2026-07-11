namespace Pdd.ir.Client.Models
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = "";
        public string Email { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Role { get; set; } = "";
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UserCreateRequest
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string Email { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Role { get; set; } = "User";
        public bool IsActive { get; set; } = true;
    }

    public class RoleDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
    }
}
