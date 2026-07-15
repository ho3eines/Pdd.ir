using Microsoft.AspNetCore.Mvc;
using Pdd.ir.Business.Models.DTOs;
using Pdd.ir.Business.Services;

namespace Pdd.ir.Server.Controllers
{
    [ApiController]
    [Route("api/permission")]
    public class PermissionController : ControllerBase
    {
        private readonly PermissionBusinessService _permService;

        public PermissionController(PermissionBusinessService permService)
        {
            _permService = permService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var perms = await _permService.GetAllAsync();
            return Ok(perms);
        }

        [HttpGet("role/{roleId}")]
        public async Task<IActionResult> GetByRoleId(int roleId)
        {
            var perms = await _permService.GetByRoleIdAsync(roleId);
            return Ok(perms);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PermissionDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Label))
                return BadRequest(new { message = "Name and Label are required" });

            var id = await _permService.CreateAsync(request);
            return Ok(new { id, message = "Permission created" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PermissionDto request)
        {
            var result = await _permService.UpdateAsync(id, request);
            if (!result) return NotFound();
            return Ok(new { message = "Permission updated" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _permService.DeleteAsync(id);
            if (!result) return NotFound();
            return Ok(new { message = "Permission deleted" });
        }
    }
}
