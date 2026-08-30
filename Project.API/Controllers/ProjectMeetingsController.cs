using Microsoft.AspNetCore.Mvc;
using Project.API.Common;
using Project.BLL.DTOs;
using Project.BLL.Services;
using System.Collections.Generic;
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
        return Ok(ApiResponse<IEnumerable<MeetingDetailsDto>>.SuccessResponse(meetings, "تم جلب الاجتماعات بنجاح."));
    }

    // 2. Get single meeting details
    [HttpGet("details/{id:int}")]
    public async Task<IActionResult> GetMeeting(int id)
    {
        var meeting = await _meetingService.GetMeetingByIdAsync(id);
        if (meeting == null)
        {
            return NotFound(ApiResponse<object>.FailureResponse("الاجتماع غير موجود", "خطأ 404"));
        }
        return Ok(ApiResponse<MeetingDetailsDto>.SuccessResponse(meeting, "تم جلب تفاصيل الاجتماع بنجاح."));
    }

    // 3. Create meeting
    [HttpPost("create")]
    public async Task<IActionResult> CreateMeeting([FromBody] CreateMeetingDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailureResponse("بيانات غير صالحة", "خطأ في التحقق"));
        }

        var createdMeeting = await _meetingService.CreateMeetingAsync(dto);
        if (createdMeeting == null)
        {
            return BadRequest(ApiResponse<object>.FailureResponse("المشروع المرتبط غير موجود", "خطأ"));
        }

        return StatusCode(201, ApiResponse<MeetingDetailsDto>.SuccessResponse(createdMeeting, "تم إنشاء الاجتماع بنجاح."));
    }

    // 4. Update meeting
    [HttpPut("update/{id:int}")]
    public async Task<IActionResult> UpdateMeeting(int id, [FromBody] UpdateMeetingDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailureResponse("بيانات غير صالحة", "خطأ في التحقق"));
        }

        var success = await _meetingService.UpdateMeetingAsync(id, dto);
        if (!success)
        {
            return NotFound(ApiResponse<object>.FailureResponse("الاجتماع أو المشروع المرتبط غير موجود", "خطأ"));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null!, "تم تحديث بيانات الاجتماع بنجاح."));
    }

    // 5. Delete meeting
    [HttpDelete("delete/{id:int}")]
    public async Task<IActionResult> DeleteMeeting(int id)
    {
        var success = await _meetingService.DeleteMeetingAsync(id);
        if (!success)
        {
            return NotFound(ApiResponse<object>.FailureResponse("الاجتماع غير موجود", "خطأ 404"));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null!, "تم حذف الاجتماع بنجاح."));
    }
}
