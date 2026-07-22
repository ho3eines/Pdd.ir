using Microsoft.AspNetCore.Mvc;
using Pdd.ir.Business.Models.DTOs;
using Pdd.ir.Business.Services;

namespace Pdd.ir.Server.Controllers
{
    [ApiController]
    [Route("api/client")]
    public class ClientController : ControllerBase
    {
        private readonly ClientBusinessService _service;

        public ClientController(ClientBusinessService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _service.GetAllAsync();
            return Ok(new { success = true, data = items });
        }

        [HttpGet("admin")]
        public async Task<IActionResult> GetAllAdmin()
        {
            var items = await _service.GetAllAdminAsync();
            return Ok(new { success = true, data = items });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound(new { success = false, message = "Client not found" });
            return Ok(new { success = true, data = item });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ClientCreateRequest request)
        {
            var id = await _service.InsertAsync(request);
            return Ok(new { success = true, data = new { id } });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ClientDto dto)
        {
            dto.Id = id;
            var success = await _service.UpdateAsync(dto);
            if (!success) return NotFound(new { success = false, message = "Client not found" });
            return Ok(new { success = true });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _service.DeleteAsync(id);
            if (!success) return NotFound(new { success = false, message = "Client not found" });
            return Ok(new { success = true });
        }
    }
}
