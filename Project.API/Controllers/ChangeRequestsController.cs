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
public class ChangeRequestsController : ControllerBase
{
    private readonly IChangeRequestService _changeRequestService;

    public ChangeRequestsController(IChangeRequestService changeRequestService)
    {
        _changeRequestService = changeRequestService;
    }

    // 1. Get all change requests
    [HttpGet("all")]
    public async Task<IActionResult> GetChangeRequests(
        [FromQuery] int? projectId,
        [FromQuery] string? keyword)
    {
        var crs = await _changeRequestService.GetChangeRequestsAsync(projectId, keyword);
        return Ok(crs);
    }

    // 2. Get single change request details
    [HttpGet("details/{id:int}")]
    public async Task<IActionResult> GetChangeRequest(int id)
    {
        var cr = await _changeRequestService.GetChangeRequestByIdAsync(id);
        if (cr == null)
        {
            return NotFound();
        }
        return Ok(cr);
    }

    // 3. Create change request
    [HttpPost("create")]
    public async Task<IActionResult> CreateChangeRequest([FromBody] CreateChangeRequestDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var createdCr = await _changeRequestService.CreateChangeRequestAsync(dto, userId);

        if (createdCr == null)
        {
            return BadRequest(new { message = $"Project with ID {dto.ProjectId} does not exist." });
        }

        return StatusCode(201, createdCr);
    }

    // 4. Update change request
    [HttpPut("update/{id:int}")]
    public async Task<IActionResult> UpdateChangeRequest(int id, [FromBody] UpdateChangeRequestDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var success = await _changeRequestService.UpdateChangeRequestAsync(id, dto);
        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    // 5. Approve change request
    [HttpPost("approve/{id:int}")]
    public async Task<IActionResult> ApproveChangeRequest(int id, [FromQuery] string? approvedById)
    {
        var success = await _changeRequestService.ApproveChangeRequestAsync(id, approvedById ?? User.FindFirstValue(ClaimTypes.NameIdentifier));
        if (!success)
        {
            return NotFound();
        }

        return Ok(new { message = "Change request approved successfully." });
    }

    // 6. Reject change request
    [HttpPost("reject/{id:int}")]
    public async Task<IActionResult> RejectChangeRequest(int id, [FromQuery] string? approvedById)
    {
        var success = await _changeRequestService.RejectChangeRequestAsync(id, approvedById ?? User.FindFirstValue(ClaimTypes.NameIdentifier));
        if (!success)
        {
            return NotFound();
        }

        return Ok(new { message = "Change request rejected successfully." });
    }

    // 7. Delete change request
    [HttpDelete("delete/{id:int}")]
    public async Task<IActionResult> DeleteChangeRequest(int id)
    {
        var success = await _changeRequestService.DeleteChangeRequestAsync(id);
        if (!success)
        {
            return NotFound();
        }

        return Ok(new { message = "Change request deleted successfully." });
    }

    // 8. Upload files for change requests
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

    // 9. Get all comments for a change request
    [HttpGet("{requestId:int}/comments")]
    public async Task<IActionResult> GetComments(int requestId)
    {
        var comments = await _changeRequestService.GetCommentsAsync(requestId);
        return Ok(comments);
    }

    // 10. Add a comment to a change request
    [HttpPost("comments")]
    public async Task<IActionResult> CreateComment([FromBody] CreateCommentDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var createdComment = await _changeRequestService.CreateCommentAsync(dto, userId);

        return Ok(createdComment);
    }
}
