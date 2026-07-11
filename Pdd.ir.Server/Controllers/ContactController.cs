using Microsoft.AspNetCore.Mvc;
using Pdd.ir.Business.Models.DTOs;
using Pdd.ir.Business.Services;

namespace Pdd.ir.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContactController : ControllerBase
    {
        private readonly ContactBusinessService _contactService;

        public ContactController(ContactBusinessService contactService)
        {
            _contactService = contactService;
        }

        [HttpPost]
        public async Task<IActionResult> Submit([FromBody] ContactRequest request)
        {
            try
            {
                var id = await _contactService.SubmitAsync(request);
                return Ok(ApiResponse<int>.Ok(id, "پیام شما ارسال شد"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var messages = await _contactService.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<ContactDto>>.Ok(messages));
        }

        [HttpGet("unread")]
        public async Task<IActionResult> GetUnread()
        {
            var messages = await _contactService.GetUnreadAsync();
            return Ok(ApiResponse<IEnumerable<ContactDto>>.Ok(messages));
        }

        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            await _contactService.MarkAsReadAsync(id);
            return Ok(ApiResponse.Ok("خوانده شد"));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _contactService.DeleteAsync(id);
            return Ok(ApiResponse.Ok("حذف شد"));
        }
    }
}
