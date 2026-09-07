using Microsoft.AspNetCore.Mvc;
using Project.BLL.DTOs;
using Project.BLL.Services;
using System.Threading.Tasks;

namespace Project.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAllCategories([FromQuery] string? keyword = null)
    {
        var categories = await _categoryService.GetCategoriesAsync(keyword);
        return Ok(categories);
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateCategory([FromBody] CategoryDto dto)
    {
        var category = await _categoryService.CreateCategoryAsync(dto);
        if (category == null)
        {
            return BadRequest("Category Name and AssignTo are required.");
        }

        return Ok(category);
    }

    [HttpPut("update/{id:int}")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryDto dto)
    {
        var category = await _categoryService.UpdateCategoryAsync(id, dto);
        if (category == null)
        {
            return NotFound("Category not found.");
        }

        return Ok(category);
    }

    [HttpDelete("delete/{id:int}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var success = await _categoryService.DeleteCategoryAsync(id);
        if (!success)
        {
            return NotFound("Category not found.");
        }

        return Ok();
    }
}
