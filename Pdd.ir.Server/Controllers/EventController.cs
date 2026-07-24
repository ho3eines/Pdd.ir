using Microsoft.AspNetCore.Mvc;
using Pdd.ir.Business.Models.DTOs;
using Pdd.ir.Business.Services;

namespace Pdd.ir.Server.Controllers
{
    [ApiController]
    [Route("api/event")]
    public class EventController : ControllerBase
    {
        private readonly EventBusinessService _service;

        public EventController(EventBusinessService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _service.GetAllAsync();
            return Ok(new { success = true, data = items });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound(new { success = false, message = "Event not found" });
            return Ok(new { success = true, data = item });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EventCreateRequest request)
        {
            var id = await _service.InsertAsync(request);
            return Ok(new { success = true, data = new { id } });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] EventDto dto)
        {
            dto.Id = id;
            var success = await _service.UpdateAsync(dto);
            if (!success) return NotFound(new { success = false, message = "Event not found" });
            return Ok(new { success = true });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _service.DeleteAsync(id);
            if (!success) return NotFound(new { success = false, message = "Event not found" });
            return Ok(new { success = true });
        }
    }
}
