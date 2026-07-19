using Microsoft.AspNetCore.Mvc;
using Pdd.ir.Business.Models.DTOs;
using Pdd.ir.Business.Services;
using Pdd.ir.Server.Services;

namespace Pdd.ir.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthBusinessService _authService;
        private readonly JwtService _jwtService;
        private readonly AesKeyStore _keyStore;
        private readonly ClientSessionService _sessionService;

        public AuthController(AuthBusinessService authService, JwtService jwtService, AesKeyStore keyStore, ClientSessionService sessionService)
        {
            _authService = authService;
            _jwtService = jwtService;
            _keyStore = keyStore;
            _sessionService = sessionService;
        }

        [HttpPost("handshake")]
        public async Task<IActionResult> Handshake([FromBody] HandshakeRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Encrypted))
                return BadRequest(new { message = "Missing encrypted payload" });

            var (success, encryptedResponse, error) = await _sessionService.HandleHandshakeAsync(request.Encrypted);

            if (!success)
                return Unauthorized(new { message = error });

            return Ok(new { encrypted = true, data = encryptedResponse });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(request.Username, request.Password);

            if (!result.Success)
                return Unauthorized(ApiResponse.Fail(result.Message));

            var user = await _authService.GetUserByUsernameAsync(request.Username);

            if (user == null)
                return Unauthorized(ApiResponse.Fail("کاربر یافت نشد"));

            var token = _jwtService.GenerateToken(user);
            var refreshToken = Guid.NewGuid().ToString("N");

            _keyStore.SetKey(request.Username, result.AesKey);

            return Ok(ApiResponse<LoginResponse>.Ok(new LoginResponse
            {
                Success = true,
                Token = token,
                RefreshToken = refreshToken,
                AesKey = result.AesKey,
                Username = user.Username,
                FullName = user.FullName,
                Role = user.Role
            }));
        }

        [HttpPost("refresh")]
        public IActionResult Refresh([FromBody] RefreshRequest request)
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
                return BadRequest(ApiResponse.Fail("Refresh token required"));

            return Ok(ApiResponse<RefreshResponse>.Ok(new RefreshResponse
            {
                Success = true,
                Token = _jwtService.GenerateToken(new Pdd.ir.Business.Models.Entities.User
                {
                    Id = 1,
                    Username = "admin",
                    Role = "Admin"
                }),
                AesKey = AuthBusinessService.GenerateAesKey()
            }));
        }

        [HttpPost("logout")]
        public IActionResult Logout([FromHeader(Name = "X-Username")] string? username)
        {
            if (!string.IsNullOrEmpty(username))
                _keyStore.RemoveKey(username);

            return Ok(ApiResponse.Ok("خروج موفق"));
        }
    }
}
