using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Project.API.Common;
using Project.BLL.DTOs;
using Project.BLL.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Project.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectMeetingsController : ControllerBase
{
    private readonly IMeetingService _meetingService;

    public ProjectMeetingsController(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }

    // 1. Get all meetings with filters
    [HttpGet("all")]
    public async Task<IActionResult> GetMeetings(
        [FromQuery] int? projectId,
        [FromQuery] string? keyword)
    {
        var meetings = await _meetingService.GetMeetingsAsync(projectId, keyword);
        return Ok(meetings); // Raw list for Angular frontend compatibility
    }

    // 2. Get single meeting details
    [HttpGet("details/{id:int}")]
    public async Task<IActionResult> GetMeeting(int id)
    {
        var meeting = await _meetingService.GetMeetingByIdAsync(id);
        if (meeting == null)
        {
            return NotFound();
        }
        return Ok(meeting);
    }

    // 3. Create meeting
    [HttpPost("create")]
    public async Task<IActionResult> CreateMeeting([FromBody] CreateMeetingDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var createdMeeting = await _meetingService.CreateMeetingAsync(dto);
        if (createdMeeting == null)
        {
            return BadRequest(new { message = "Project not found" });
        }

        return StatusCode(201, createdMeeting);
    }

    // 4. Update meeting
    [HttpPut("update/{id:int}")]
    public async Task<IActionResult> UpdateMeeting(int id, [FromBody] UpdateMeetingDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var success = await _meetingService.UpdateMeetingAsync(id, dto);
        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    // 5. Delete meeting
    [HttpDelete("delete/{id:int}")]
    public async Task<IActionResult> DeleteMeeting(int id)
    {
        var success = await _meetingService.DeleteMeetingAsync(id);
        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    // 6. Upload files for meetings
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
