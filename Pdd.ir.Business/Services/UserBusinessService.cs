using Pdd.ir.Business.Models.DTOs;
using Pdd.ir.Business.Models.Entities;

namespace Pdd.ir.Business.Services
{
    public class UserBusinessService
    {
        private readonly AuthBusinessService _auth;

        public UserBusinessService(AuthBusinessService auth)
        {
            _auth = auth;
        }

        public async Task<IEnumerable<UserDto>> GetAllAsync()
        {
            var users = await _auth.GetAllUsersAdminAsync();
            return users.Select(MapToDto);
        }

        public async Task<UserDto?> GetByIdAsync(int id)
        {
            var user = await _auth.GetUserByIdAsync(id);
            return user != null ? MapToDto(user) : null;
        }

        public async Task<int> InsertAsync(CreateUserDto dto)
        {
            return await _auth.CreateUserAsync(dto.Username, dto.Password, dto.Email, dto.FullName, dto.Role);
        }

        public async Task<bool> UpdateAsync(UserDto dto)
        {
            return await _auth.UpdateUserAsync(dto.Id, dto.Email, dto.FullName, dto.Role);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _auth.DeleteUserAsync(id);
        }

        private static UserDto MapToDto(User u)
        {
            return new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                FullName = u.FullName,
                Role = u.Role,
                IsActive = u.IsActive
            };
        }
    }

    public class CreateUserDto
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string Email { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Role { get; set; } = "User";
    }
}
