using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
public class ProjectTasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public ProjectTasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    // 1. Get tasks list with filters
    [HttpGet("all")]
    public async Task<ActionResult<IEnumerable<TaskDetailsDto>>> GetTasks(
        [FromQuery] int? projectId,
        [FromQuery] string? status,
        [FromQuery] string? keyword)
    {
        var tasks = await _taskService.GetTasksAsync(projectId, status, keyword);
        return Ok(tasks);
    }

    // 2. Get single task details
    [HttpGet("details/{id}")]
    public async Task<ActionResult<TaskDetailsDto>> GetTask(int id)
    {
        var task = await _taskService.GetTaskByIdAsync(id);
        if (task == null)
            return NotFound();

        return Ok(task);
    }

    // 3. Create task
    [HttpPost("create")]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var (createdTask, errorMessage) = await _taskService.CreateTaskAsync(dto, currentUserId);

        if (!string.IsNullOrEmpty(errorMessage))
            return BadRequest(new { message = errorMessage });

        return CreatedAtAction(nameof(GetTask), new { id = createdTask!.Id }, createdTask);
    }

    // 4. Update task
    [HttpPut("update/{id}")]
    public async Task<IActionResult> UpdateTask(int id, [FromBody] UpdateTaskDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var (success, errorMessage) = await _taskService.UpdateTaskAsync(id, dto);
        if (!success)
        {
            if (errorMessage == "Task not found.")
                return NotFound();

            return BadRequest(new { message = errorMessage });
        }

        return NoContent();
    }

    // 5. Delete task
    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var success = await _taskService.DeleteTaskAsync(id);
        if (!success)
            return NotFound();

        return Ok(new { message = "Task deleted successfully." });
    }

    // 6. Upload files for tasks
    [HttpPost("upload")]
    public async Task<IActionResult> UploadFiles(List<IFormFile> files)
    {
        if (files == null || files.Count == 0)
            return BadRequest("No files uploaded.");

        var uploadedFilesList = new List<object>();
        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

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

                uploadedFilesList.Add(new
                {
                    originalName = originalName,
                    uniqueName = uniqueName,
                    filePath = $"/uploads/{uniqueName}"
                });
            }
        }

        return Ok(uploadedFilesList);
    }
}