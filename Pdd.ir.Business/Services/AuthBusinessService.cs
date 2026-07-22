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
            // Generate random salt (16 bytes)
            var salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Hash with PBKDF2-SHA256 (100,000 iterations)
            var hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterations: 100000,
                HashAlgorithmName.SHA256,
                outputLength: 32);

            // Combine salt + hash and encode
            var result = new byte[16 + 32];
            Buffer.BlockCopy(salt, 0, result, 0, 16);
            Buffer.BlockCopy(hash, 0, result, 16, 32);
            return Convert.ToBase64String(result);
        }

        public static bool VerifyPassword(string password, string hash)
        {
            try
            {
                var hashBytes = Convert.FromBase64String(hash);

                // Check if this is a PBKDF2 hash (48 bytes: 16 salt + 32 hash)
                if (hashBytes.Length == 48)
                {
                    return VerifyPbkdf2Password(password, hashBytes);
                }
                // Legacy SHA-256 hash (32 bytes)
                else if (hashBytes.Length == 32)
                {
                    return VerifySha256Password(password, hash);
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private static bool VerifyPbkdf2Password(string password, byte[] hashBytes)
        {
            // Extract salt (first 16 bytes)
            var salt = new byte[16];
            Buffer.BlockCopy(hashBytes, 0, salt, 0, 16);

            // Extract stored hash (remaining 32 bytes)
            var storedHash = new byte[32];
            Buffer.BlockCopy(hashBytes, 16, storedHash, 0, 32);

            // Hash input password with same salt
            var computedHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterations: 100000,
                HashAlgorithmName.SHA256,
                outputLength: 32);

            // Compare using constant-time comparison
            return CryptographicOperations.FixedTimeEquals(computedHash, storedHash);
        }

        private static bool VerifySha256Password(string password, string hash)
        {
            // Legacy SHA-256 verification
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            var computedHash = Convert.ToBase64String(bytes);
            return computedHash == hash;
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
