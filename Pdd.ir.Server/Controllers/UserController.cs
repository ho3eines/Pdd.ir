using Microsoft.AspNetCore.Mvc;
using Pdd.ir.Business.Models.DTOs;
using Pdd.ir.Business.Services;

namespace Pdd.ir.Server.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private readonly AuthBusinessService _authService;

        public UserController(AuthBusinessService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _authService.GetAllUsersAdminAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _authService.GetUserByIdAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { message = "Username and password are required" });

            var existing = await _authService.GetAllUsersAsync();
            if (existing.Any(u => u.Username == request.Username))
                return BadRequest(new { message = "Username already exists" });

            var id = await _authService.CreateUserAsync(
                request.Username,
                request.Password,
                request.Email,
                request.FullName,
                request.Role);

            return Ok(new { id, message = "User created successfully" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest request)
        {
            var result = await _authService.UpdateUserAsync(id, request.Email, request.FullName, request.Role);
            if (!result) return NotFound();
            return Ok(new { message = "User updated successfully" });
        }

        [HttpPut("{id}/password")]
        public async Task<IActionResult> UpdatePassword(int id, [FromBody] UpdatePasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { message = "Password is required" });

            var result = await _authService.UpdateUserPasswordAsync(id, request.Password);
            if (!result) return NotFound();
            return Ok(new { message = "Password updated successfully" });
        }

        [HttpPut("{id}/toggle")]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var result = await _authService.ToggleUserActiveAsync(id);
            if (!result) return NotFound();
            return Ok(new { message = "User status toggled" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _authService.DeleteUserAsync(id);
            if (!result) return NotFound();
            return Ok(new { message = "User deleted" });
        }
    }

    public class CreateUserRequest
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string Email { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Role { get; set; } = "User";
    }

    public class UpdateUserRequest
    {
        public string Email { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Role { get; set; } = "User";
    }

    public class UpdatePasswordRequest
    {
        public string Password { get; set; } = "";
    }
}
