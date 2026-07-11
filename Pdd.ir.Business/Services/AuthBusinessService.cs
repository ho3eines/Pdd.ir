using Pdd.ir.Business.Models.DTOs;
using Pdd.ir.Business.Models.Entities;
using Pdd.ir.Data;
using Pdd.ir.Data.Queries;
using System.Security.Cryptography;
using System.Text;

namespace Pdd.ir.Business.Services
{
    public class AuthBusinessService
    {
        private readonly IDbService _db;

        public AuthBusinessService(IDbService db)
        {
            _db = db;
        }

        public async Task<LoginResponse> LoginAsync(string username, string password)
        {
            var user = await _db.QueryFirstOrDefaultAsync<User>(UserQueries.GetByUsername, new { Username = username });

            if (user == null)
                return new LoginResponse { Success = false, Message = "نام کاربری یا رمز عبور اشتباه است" };

            if (!VerifyPassword(password, user.PasswordHash))
                return new LoginResponse { Success = false, Message = "نام کاربری یا رمز عبور اشتباه است" };

            var aesKey = GenerateAesKey();

            return new LoginResponse
            {
                Success = true,
                Message = "ورود موفق",
                Username = user.Username,
                FullName = user.FullName,
                Role = user.Role,
                AesKey = aesKey
            };
        }

        public async Task<int> CreateUserAsync(string username, string password, string email, string fullName, string role = "User")
        {
            var hash = HashPassword(password);
            return await _db.ExecuteScalarAsync<int>(UserQueries.Insert, new
            {
                Username = username,
                PasswordHash = hash,
                Email = email,
                FullName = fullName,
                Role = role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _db.QueryFirstOrDefaultAsync<User>(UserQueries.GetById, new { Id = id });
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _db.QueryAsync<User>(UserQueries.GetAll);
        }

        public static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        public static bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }

        public static string GenerateAesKey()
        {
            var key = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(key);
            return Convert.ToBase64String(key);
        }
    }
}
