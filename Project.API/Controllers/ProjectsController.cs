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
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    // 1. Get projects list with filters
    [HttpGet("all")]
    public async Task<IActionResult> GetProjects(
        [FromQuery] int? portfolioId,
        [FromQuery] int? programId,
        [FromQuery] string? keyword,
        [FromQuery] string? status)
    {
        var projects = await _projectService.GetProjectsAsync(portfolioId, programId, keyword, status);
        return Ok(ApiResponse<IEnumerable<ProjectDetailsDto>>.SuccessResponse(projects, "تم جلب المشاريع بنجاح."));
    }

    // 2. Get project by ID
    [HttpGet("details/{id:int}")]
    public async Task<IActionResult> GetProjectById(int id)
    {
        var project = await _projectService.GetProjectByIdAsync(id);
        if (project == null)
        {
            return NotFound(ApiResponse<object>.FailureResponse("المشروع غير موجود", "خطأ 404"));
        }
        return Ok(ApiResponse<ProjectDetailsDto>.SuccessResponse(project, "تم جلب تفاصيل المشروع بنجاح."));
    }

    // 3. Create project
    [HttpPost("create")]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailureResponse("بيانات غير صالحة", "خطأ في التحقق"));
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "default-user-id";
        var createdProject = await _projectService.CreateProjectAsync(dto, userId);

        if (createdProject == null)
        {
            return BadRequest(ApiResponse<object>.FailureResponse("المحفظة أو البرنامج المرتبط غير موجود", "خطأ"));
        }

        return StatusCode(201, ApiResponse<ProjectDetailsDto>.SuccessResponse(createdProject, "تم إنشاء المشروع بنجاح."));
    }

    // 4. Update project
    [HttpPut("update/{id:int}")]
    public async Task<IActionResult> UpdateProject(int id, [FromBody] UpdateProjectDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailureResponse("بيانات غير صالحة", "خطأ في التحقق"));
        }

        var success = await _projectService.UpdateProjectAsync(id, dto);
        if (!success)
        {
            return NotFound(ApiResponse<object>.FailureResponse("المشروع أو المحفظة/البرنامج المرتبط غير موجود", "خطأ"));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null!, "تم تحديث بيانات المشروع بنجاح."));
    }

    // 5. Delete project
    [HttpDelete("delete/{id:int}")]
    public async Task<IActionResult> DeleteProject(int id)
    {
        var success = await _projectService.DeleteProjectAsync(id);
        if (!success)
        {
            return NotFound(ApiResponse<object>.FailureResponse("المشروع غير موجود", "خطأ 404"));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null!, "تم حذف المشروع بنجاح."));
    }
}
