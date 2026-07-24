using Microsoft.AspNetCore.Mvc;
using Pdd.ir.Business.Models.DTOs;
using Pdd.ir.Business.Services;

namespace Pdd.ir.Server.Controllers
{
    [ApiController]
    [Route("api/homeslide")]
    public class HomeSlideController : ControllerBase
    {
        private readonly HomeSlideBusinessService _service;

        public HomeSlideController(HomeSlideBusinessService service) { _service = service; }

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
            if (item == null) return NotFound(new { success = false, message = "Slide not found" });
            return Ok(new { success = true, data = item });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] HomeSlideCreateRequest request)
        {
            var id = await _service.InsertAsync(request);
            return Ok(new { success = true, data = new { id } });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] HomeSlideDto dto)
        {
            dto.Id = id;
            var ok = await _service.UpdateAsync(dto);
            if (!ok) return NotFound(new { success = false, message = "Slide not found" });
            return Ok(new { success = true });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.DeleteAsync(id);
            if (!ok) return NotFound(new { success = false, message = "Slide not found" });
            return Ok(new { success = true });
        }
    }
}
