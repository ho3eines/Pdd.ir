namespace Pdd.ir.Client.Models
{
    public class LoginRequest
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class LoginResponse
    {
        public string Token { get; set; } = "";
        public string RefreshToken { get; set; } = "";
        public string Username { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Role { get; set; } = "";
        public string AesKey { get; set; } = "";
    }

    public class RefreshRequest
    {
        public string RefreshToken { get; set; } = "";
    }

    public class RefreshResponse
    {
        public string Token { get; set; } = "";
        public string RefreshToken { get; set; } = "";
    }
}
