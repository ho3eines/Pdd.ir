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

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _db.QueryFirstOrDefaultAsync<User>(UserQueries.GetByUsername, new { Username = username });
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _db.QueryAsync<User>(UserQueries.GetAll);
        }

        public async Task<IEnumerable<User>> GetAllUsersAdminAsync()
        {
            return await _db.QueryAsync<User>(UserQueries.GetAllAdmin);
        }

        public async Task<bool> UpdateUserAsync(int id, string email, string fullName, string role)
        {
            var rows = await _db.ExecuteAsync(UserQueries.Update, new { Id = id, Email = email, FullName = fullName, Role = role, IsActive = true });
            return rows > 0;
        }

        public async Task<bool> UpdateUserPasswordAsync(int id, string newPassword)
        {
            var hash = HashPassword(newPassword);
            var rows = await _db.ExecuteAsync(UserQueries.UpdatePassword, new { Id = id, PasswordHash = hash });
            return rows > 0;
        }

        public async Task<bool> ToggleUserActiveAsync(int id)
        {
            var user = await _db.QueryFirstOrDefaultAsync<User>(UserQueries.GetById, new { Id = id });
            if (user == null) return false;
            var newStatus = !user.IsActive;
            var rows = await _db.ExecuteAsync(UserQueries.SetActive, new { Id = id, IsActive = newStatus });
            return rows > 0;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var rows = await _db.ExecuteAsync(UserQueries.Delete, new { Id = id });
            return rows > 0;
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
