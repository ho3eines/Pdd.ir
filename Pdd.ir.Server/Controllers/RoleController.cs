using Microsoft.AspNetCore.Mvc;
using Pdd.ir.Business.Models.DTOs;
using Pdd.ir.Business.Services;

namespace Pdd.ir.Server.Controllers
{
    [ApiController]
    [Route("api/role")]
    public class RoleController : ControllerBase
    {
        private readonly RoleBusinessService _roleService;

        public RoleController(RoleBusinessService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var roles = await _roleService.GetAllAsync();
            return Ok(roles);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var role = await _roleService.GetByIdAsync(id);
            if (role == null) return NotFound();
            return Ok(role);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RoleCreateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { message = "Name is required" });

            var existing = await _roleService.GetAllAsync();
            if (existing.Any(r => r.Name == request.Name))
                return BadRequest(new { message = "Role name already exists" });

            var id = await _roleService.CreateAsync(request);
            return Ok(new { id, message = "Role created" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] RoleUpdateRequest request)
        {
            var result = await _roleService.UpdateAsync(id, request);
            if (!result) return NotFound();
            return Ok(new { message = "Role updated" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var count = await _roleService.CountUsersAsync("");
            var role = await _roleService.GetByIdAsync(id);
            if (role == null) return NotFound();

            var userCount = await _roleService.CountUsersAsync(role.Name);
            if (userCount > 0)
                return BadRequest(new { message = $"Cannot delete role: {userCount} users assigned" });

            var result = await _roleService.DeleteAsync(id);
            if (!result) return NotFound();
            return Ok(new { message = "Role deleted" });
        }
    }
}
