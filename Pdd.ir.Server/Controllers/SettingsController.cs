using Microsoft.AspNetCore.Mvc;
using Pdd.ir.Business.Services;

namespace Pdd.ir.Server.Controllers
{
    [ApiController]
    [Route("api/settings")]
    public class SettingsController : ControllerBase
    {
        private readonly SettingsBusinessService _service;

        public SettingsController(SettingsBusinessService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var settings = await _service.GetAllAsync();
            return Ok(new { success = true, data = settings });
        }

        [HttpGet("{key}")]
        public async Task<IActionResult> Get(string key)
        {
            var value = await _service.GetAsync(key);
            if (value == null) return NotFound();
            return Ok(new { success = true, data = new { key, value } });
        }

        [HttpPost]
        public async Task<IActionResult> Set([FromBody] SettingRequest request)
        {
            await _service.SetAsync(request.Key, request.Value);
            return Ok(new { success = true });
        }

        [HttpPost("bulk")]
        public async Task<IActionResult> SetBulk([FromBody] Dictionary<string, string> settings)
        {
            await _service.SetManyAsync(settings);
            return Ok(new { success = true });
        }
    }

    public class SettingRequest
    {
        public string Key { get; set; } = "";
        public string Value { get; set; } = "";
    }
}
