using Microsoft.AspNetCore.Authorization;
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
public class ProjectTasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public ProjectTasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    // 1. Get tasks list with filters
    [HttpGet("all")]
    public async Task<IActionResult> GetTasks(
        [FromQuery] int? projectId,
        [FromQuery] string? status,
        [FromQuery] string? keyword)
    {
        var tasks = await _taskService.GetTasksAsync(projectId, status, keyword);
        return Ok(ApiResponse<IEnumerable<TaskDetailsDto>>.SuccessResponse(tasks, "تم جلب المهام بنجاح."));
    }

    // 2. Get single task details
    [HttpGet("details/{id:int}")]
    public async Task<IActionResult> GetTask(int id)
    {
        var task = await _taskService.GetTaskByIdAsync(id);
        if (task == null)
        {
            return NotFound(ApiResponse<object>.FailureResponse("المهمة غير موجودة", "خطأ 404"));
        }
        return Ok(ApiResponse<TaskDetailsDto>.SuccessResponse(task, "تم جلب تفاصيل المهمة بنجاح."));
    }

    // 3. Create task
    [HttpPost("create")]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailureResponse("بيانات غير صالحة", "خطأ في التحقق"));
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "default-user-id";
        var createdTask = await _taskService.CreateTaskAsync(dto, userId);

        if (createdTask == null)
        {
            return BadRequest(ApiResponse<object>.FailureResponse("المشروع المرتبط غير موجود", "خطأ"));
        }

        return StatusCode(201, ApiResponse<TaskDetailsDto>.SuccessResponse(createdTask, "تم إنشاء المهمة بنجاح."));
    }

    // 4. Update task
    [HttpPut("update/{id:int}")]
    public async Task<IActionResult> UpdateTask(int id, [FromBody] UpdateTaskDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailureResponse("بيانات غير صالحة", "خطأ في التحقق"));
        }

        var success = await _taskService.UpdateTaskAsync(id, dto);
        if (!success)
        {
            return NotFound(ApiResponse<object>.FailureResponse("المهمة أو المشروع المرتبط غير موجود", "خطأ"));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null!, "تم تحديث بيانات المهمة بنجاح."));
    }

    // 5. Delete task
    [HttpDelete("delete/{id:int}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var success = await _taskService.DeleteTaskAsync(id);
        if (!success)
        {
            return NotFound(ApiResponse<object>.FailureResponse("المهمة غير موجودة", "خطأ 404"));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null!, "تم حذف المهمة بنجاح."));
    }

    // 6. Upload files for tasks
    [HttpPost("upload")]
    public async Task<IActionResult> UploadFiles(List<IFormFile> files)
    {
        if (files == null || files.Count == 0)
        {
            return BadRequest(ApiResponse<object>.FailureResponse("لم يتم رفع أي ملفات", "خطأ"));
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

                uploadedFilesList.Add(new
                {
                    originalName = originalName,
                    uniqueName = uniqueName,
                    filePath = $"/uploads/{uniqueName}"
                });
            }
        }

        return Ok(ApiResponse<object>.SuccessResponse(uploadedFilesList, "تم رفع الملفات بنجاح."));
    }
}
