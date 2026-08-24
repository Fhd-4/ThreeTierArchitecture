using Microsoft.AspNetCore.Mvc;
using Project.API.Common;
using Project.BLL.DTOs;
using Project.BLL.Services;

namespace Project.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;

    public ProductsController(IProductService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _service.GetProductsAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<ProductDto>>.SuccessResponse(result, "تم جلب قائمة المنتجات بنجاح"));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        var result = await _service.GetProductByIdAsync(id, cancellationToken);
        if (result == null)
        {
            return NotFound(ApiResponse<ProductDto>.FailureResponse(
                $"المنتج ذو المعرف {id} غير موجود", "غير موجود"));
        }

        return Ok(ApiResponse<ProductDto>.SuccessResponse(result, "تم جلب تفاصيل المنتج بنجاح"));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.CreateProductAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, 
            ApiResponse<ProductDto>.SuccessResponse(result, "تم إنشاء المنتج بنجاح"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateProductDto dto, CancellationToken cancellationToken)
    {
        await _service.UpdateProductAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null!, "تم تعديل بيانات المنتج بنجاح"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        await _service.DeleteProductAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null!, "تم حذف المنتج بنجاح"));
    }
}