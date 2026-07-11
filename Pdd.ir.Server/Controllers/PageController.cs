using Microsoft.AspNetCore.Mvc;
using Pdd.ir.Business.Models.DTOs;
using Pdd.ir.Business.Services;

namespace Pdd.ir.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PageController : ControllerBase
    {
        private readonly PageBusinessService _pageService;

        public PageController(PageBusinessService pageService)
        {
            _pageService = pageService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var pages = await _pageService.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<PageDto>>.Ok(pages));
        }

        [HttpGet("{slug}")]
        public async Task<IActionResult> GetBySlug(string slug)
        {
            var page = await _pageService.GetBySlugAsync(slug);
            if (page == null)
                return NotFound(ApiResponse.Fail("صفحه یافت نشد"));

            return Ok(ApiResponse<PageDto>.Ok(page));
        }

        [HttpGet("id/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var page = await _pageService.GetByIdAsync(id);
            if (page == null)
                return NotFound(ApiResponse.Fail("صفحه یافت نشد"));

            return Ok(ApiResponse<PageDto>.Ok(page));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PageUpdateRequest request)
        {
            var id = await _pageService.CreateAsync(request);
            return Ok(ApiResponse<int>.Ok(id, "صفحه ایجاد شد"));
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] PageUpdateRequest request)
        {
            var result = await _pageService.UpdateAsync(request);
            if (!result)
                return NotFound(ApiResponse.Fail("صفحه یافت نشد"));

            return Ok(ApiResponse.Ok("صفحه بروزرسانی شد"));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _pageService.DeleteAsync(id);
            if (!result)
                return NotFound(ApiResponse.Fail("صفحه یافت نشد"));

            return Ok(ApiResponse.Ok("صفحه حذف شد"));
        }
    }
}
