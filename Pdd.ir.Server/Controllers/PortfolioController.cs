using Microsoft.AspNetCore.Mvc;
using Pdd.ir.Business.Models.DTOs;
using Pdd.ir.Business.Services;

namespace Pdd.ir.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PortfolioController : ControllerBase
    {
        private readonly PortfolioBusinessService _portfolioService;

        public PortfolioController(PortfolioBusinessService portfolioService)
        {
            _portfolioService = portfolioService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _portfolioService.GetAllAsync();
            return Ok(new ApiResponse<IEnumerable<PortfolioDto>> { Success = true, Data = items });
        }

        [HttpGet("admin")]
        public async Task<IActionResult> GetAllAdmin()
        {
            var items = await _portfolioService.GetAllAdminAsync();
            return Ok(new ApiResponse<IEnumerable<PortfolioDto>> { Success = true, Data = items });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _portfolioService.GetByIdAsync(id);
            if (item == null)
                return NotFound(new ApiResponse { Success = false, Message = "Portfolio item not found" });

            return Ok(new ApiResponse<PortfolioDto> { Success = true, Data = item });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PortfolioCreateRequest request)
        {
            var id = await _portfolioService.CreateAsync(request);
            return Ok(new ApiResponse<int> { Success = true, Data = id, Message = "Portfolio item created" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PortfolioCreateRequest request)
        {
            var result = await _portfolioService.UpdateAsync(id, request);
            if (!result)
                return NotFound(new ApiResponse { Success = false, Message = "Portfolio item not found" });

            return Ok(new ApiResponse { Success = true, Message = "Portfolio item updated" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _portfolioService.DeleteAsync(id);
            if (!result)
                return NotFound(new ApiResponse { Success = false, Message = "Portfolio item not found" });

            return Ok(new ApiResponse { Success = true, Message = "Portfolio item deleted" });
        }
    }
}
