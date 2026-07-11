using Microsoft.AspNetCore.Mvc;
using Pdd.ir.Business.Models.DTOs;
using Pdd.ir.Business.Services;

namespace Pdd.ir.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BlogController : ControllerBase
    {
        private readonly BlogBusinessService _blogService;

        public BlogController(BlogBusinessService blogService)
        {
            _blogService = blogService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var posts = await _blogService.GetAllAsync();
            return Ok(new ApiResponse<IEnumerable<BlogDto>> { Success = true, Data = posts });
        }

        [HttpGet("admin")]
        public async Task<IActionResult> GetAllAdmin()
        {
            var posts = await _blogService.GetAllAdminAsync();
            return Ok(new ApiResponse<IEnumerable<BlogDto>> { Success = true, Data = posts });
        }

        [HttpGet("{slug}")]
        public async Task<IActionResult> GetBySlug(string slug)
        {
            var post = await _blogService.GetBySlugAsync(slug);
            if (post == null)
                return NotFound(new ApiResponse { Success = false, Message = "Blog post not found" });

            await _blogService.IncrementViewCountAsync(post.Id);
            return Ok(new ApiResponse<BlogDto> { Success = true, Data = post });
        }

        [HttpGet("id/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var post = await _blogService.GetByIdAsync(id);
            if (post == null)
                return NotFound(new ApiResponse { Success = false, Message = "Blog post not found" });

            return Ok(new ApiResponse<BlogDto> { Success = true, Data = post });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BlogCreateRequest request)
        {
            var id = await _blogService.CreateAsync(request);
            return Ok(new ApiResponse<int> { Success = true, Data = id, Message = "Blog post created" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] BlogCreateRequest request)
        {
            var result = await _blogService.UpdateAsync(id, request);
            if (!result)
                return NotFound(new ApiResponse { Success = false, Message = "Blog post not found" });

            return Ok(new ApiResponse { Success = true, Message = "Blog post updated" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _blogService.DeleteAsync(id);
            if (!result)
                return NotFound(new ApiResponse { Success = false, Message = "Blog post not found" });

            return Ok(new ApiResponse { Success = true, Message = "Blog post deleted" });
        }
    }
}
