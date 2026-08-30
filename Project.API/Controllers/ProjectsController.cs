using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Project.API.Common;
using Project.BLL.DTOs;
using Project.BLL.Services;
using System;
using System.Collections.Generic;
using System.IO;
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
        return Ok(projects); // Raw list for Angular frontend compatibility
    }

    // 2. Get project by ID
    [HttpGet("details/{id:int}")]
    public async Task<IActionResult> GetProjectById(int id)
    {
        var project = await _projectService.GetProjectByIdAsync(id);
        if (project == null)
        {
            return NotFound();
        }
        return Ok(project);
    }

    // 3. Create project
    [HttpPost("create")]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "default-user-id";
        var createdProject = await _projectService.CreateProjectAsync(dto, userId);

        if (createdProject == null)
        {
            return BadRequest(new { message = "Portfolio or Program not found" });
        }

        return StatusCode(201, createdProject);
    }

    // 4. Update project
    [HttpPut("update/{id:int}")]
    public async Task<IActionResult> UpdateProject(int id, [FromBody] UpdateProjectDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var success = await _projectService.UpdateProjectAsync(id, dto);
        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    // 5. Delete project
    [HttpDelete("delete/{id:int}")]
    public async Task<IActionResult> DeleteProject(int id)
    {
        var success = await _projectService.DeleteProjectAsync(id);
        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    // 6. Upload files for projects
    [HttpPost("upload")]
    public async Task<IActionResult> UploadFiles(List<IFormFile> files)
    {
        if (files == null || files.Count == 0)
        {
            return BadRequest("No files uploaded.");
        }

        var uploadedFilesList = new List<object>();
        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        foreach (var file in files)
        {
            if (file.Length > 0)
            {
                var originalName = file.FileName;
                var uniqueName = Guid.NewGuid().ToString() + Path.GetExtension(originalName);
                var filePath = Path.Combine(uploadsFolder, uniqueName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var sizeInMb = (file.Length / (1024.0 * 1024.0)).ToString("F1") + " MB";
                var ext = Path.GetExtension(originalName).TrimStart('.').ToLower();

                uploadedFilesList.Add(new
                {
                    name = originalName,
                    path = "/uploads/" + uniqueName,
                    size = sizeInMb,
                    type = ext
                });
            }
        }

        return Ok(uploadedFilesList);
    }
}
