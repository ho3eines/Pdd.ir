using Microsoft.AspNetCore.Mvc;
using Pdd.ir.Business.Models.DTOs;
using Pdd.ir.Business.Services;

namespace Pdd.ir.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly ProductBusinessService _productService;

        public ProductController(ProductBusinessService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _productService.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<ProductDto>>.Ok(products));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
                return NotFound(ApiResponse.Fail("محصول یافت نشد"));

            return Ok(ApiResponse<ProductDto>.Ok(product));
        }

        [HttpGet("category/{category}")]
        public async Task<IActionResult> GetByCategory(string category)
        {
            var products = await _productService.GetByCategoryAsync(category);
            return Ok(ApiResponse<IEnumerable<ProductDto>>.Ok(products));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductCreateRequest request)
        {
            var id = await _productService.CreateAsync(request);
            return Ok(ApiResponse<int>.Ok(id, "محصول ایجاد شد"));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductCreateRequest request)
        {
            var result = await _productService.UpdateAsync(id, request);
            if (!result)
                return NotFound(ApiResponse.Fail("محصول یافت نشد"));

            return Ok(ApiResponse.Ok("محصول بروزرسانی شد"));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _productService.DeleteAsync(id);
            if (!result)
                return NotFound(ApiResponse.Fail("محصول یافت نشد"));

            return Ok(ApiResponse.Ok("محصول حذف شد"));
        }
    }
}
