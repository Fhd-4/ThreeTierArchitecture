using Microsoft.AspNetCore.Mvc;
using Project.API.Common;
using Project.API.Models;
using Serilog;

namespace Project.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            return BadRequest(ApiResponse<object>.FailureResponse("اسم المستخدم مطلوب", "بيانات غير صالحة"));
        }

        var timeString = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
        
        // كتابة حدث تسجيل الدخول المنسق داخل اللوج لتمكين محرك السجلات من تتبعه وقراءته
        Log.Information("User LOGIN: '{Username}' logged in at {Time}", request.Username, timeString);

        return Ok(ApiResponse<object>.SuccessResponse(null!, $"تم تسجيل دخول المستخدم '{request.Username}' بنجاح."));
    }

    [HttpPost("logout")]
    public IActionResult Logout([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            return BadRequest(ApiResponse<object>.FailureResponse("اسم المستخدم مطلوب", "بيانات غير صالحة"));
        }

        var timeString = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

        // كتابة حدث تسجيل الخروج المنسق داخل اللوج
        Log.Information("User LOGOUT: '{Username}' logged out at {Time}", request.Username, timeString);

        return Ok(ApiResponse<object>.SuccessResponse(null!, $"تم تسجيل خروج المستخدم '{request.Username}' بنجاح."));
    }
}
