using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.API.Common;
using Project.BLL.DTOs;
using Project.BLL.Services;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Project.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PortfoliosController : ControllerBase
{
    private readonly IPortfolioService _portfolioService;

    public PortfoliosController(IPortfolioService portfolioService)
    {
        _portfolioService = portfolioService;
    }

    // 1. Get all portfolios
    [HttpGet]
    public async Task<IActionResult> GetPortfolios()
    {
        var portfolios = await _portfolioService.GetPortfoliosAsync();
        return Ok(ApiResponse<IEnumerable<PortfolioDetailsDto>>.SuccessResponse(portfolios, "تم جلب المحافظ بنجاح."));
    }

    // 2. Get single portfolio by ID
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetPortfolio(int id)
    {
        var portfolio = await _portfolioService.GetPortfolioByIdAsync(id);
        if (portfolio == null)
        {
            return NotFound(ApiResponse<object>.FailureResponse("المحفظة غير موجودة", "خطأ 404"));
        }
        return Ok(ApiResponse<PortfolioDetailsDto>.SuccessResponse(portfolio, "تم جلب تفاصيل المحفظة بنجاح."));
    }

    // 3. Create a portfolio
    [HttpPost]
    public async Task<IActionResult> CreatePortfolio([FromBody] CreatePortfolioDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailureResponse("بيانات غير صالحة", "خطأ في التحقق"));
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "default-user-id";
        var createdPortfolio = await _portfolioService.CreatePortfolioAsync(dto, userId);

        return StatusCode(201, ApiResponse<PortfolioDetailsDto>.SuccessResponse(createdPortfolio, "تم إنشاء المحفظة بنجاح."));
    }

    // 4. Update a portfolio
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdatePortfolio(int id, [FromBody] UpdatePortfolioDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailureResponse("بيانات غير صالحة", "خطأ في التحقق"));
        }

        var success = await _portfolioService.UpdatePortfolioAsync(id, dto);
        if (!success)
        {
            return NotFound(ApiResponse<object>.FailureResponse("المحفظة غير موجودة أو فشل التعديل", "خطأ"));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null!, "تم تحديث بيانات المحفظة بنجاح."));
    }

    // 5. Delete a portfolio
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeletePortfolio(int id)
    {
        var success = await _portfolioService.DeletePortfolioAsync(id);
        if (!success)
        {
            return NotFound(ApiResponse<object>.FailureResponse("المحفظة غير موجودة", "خطأ 404"));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null!, "تم حذف المحفظة بنجاح."));
    }
}
