using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Project.API.Common;
using Project.API.Models;
using System.IO;
using System.Text.RegularExpressions;

namespace Project.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LogsController : ControllerBase
{
    private readonly IWebHostEnvironment _env;

    public LogsController(IWebHostEnvironment env)
    {
        _env = env;
    }

    [HttpGet]
    public IActionResult GetLogs()
    {
        var today = DateTime.UtcNow.ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
        var logFileName = $"log-{today}.txt";
        var logFilePath = Path.Combine(_env.ContentRootPath, "logs", logFileName);

        if (!System.IO.File.Exists(logFilePath))
        {
            return Ok(ApiResponse<List<string>>.SuccessResponse(new List<string>(), $"ملف اللوج غير موجود في المسار: {logFilePath}"));
        }

        try
        {
            using var fileStream = new FileStream(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var streamReader = new StreamReader(fileStream);

            var logsList = new List<string>();
            string? line;

            while ((line = streamReader.ReadLine()) != null)
            {
                logsList.Add(line);
            }

            return Ok(ApiResponse<List<string>>.SuccessResponse(logsList, "تم جلب سجلات النظام بنجاح."));
        }
        catch (IOException ex)
        {
            return StatusCode(500, ApiResponse<object>.FailureResponse(
                ex.Message, "فشل في قراءة ملف السجلات بسبب قيود النظام."));
        }
    }

    // نقطة اتصال جديدة لتصفية وقراءة حركات الدخول والخروج من ملف اللوج
    [HttpGet("logins")]
    public IActionResult GetLoginActivities()
    {
        var today = DateTime.UtcNow.ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
        var logFileName = $"log-{today}.txt";
        var logFilePath = Path.Combine(_env.ContentRootPath, "logs", logFileName);

        if (!System.IO.File.Exists(logFilePath))
        {
            return Ok(ApiResponse<List<UserActivityDto>>.SuccessResponse(new List<UserActivityDto>(), "لا توجد سجلات دخول لهذا اليوم."));
        }

        try
        {
            using var fileStream = new FileStream(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var streamReader = new StreamReader(fileStream);

            var activities = new List<UserActivityDto>();
            string? line;

            // نمط التعبيرات المنتظمة (Regex) للتعرف على حركات المستخدمين المسجلة في اللوج
            var loginRegex = new Regex(@"User (LOGIN|LOGOUT): '([^']+)' logged \w+ at ([\d\-:\s]+)");

            while ((line = streamReader.ReadLine()) != null)
            {
                var match = loginRegex.Match(line);
                if (match.Success)
                {
                    activities.Add(new UserActivityDto
                    {
                        Action = match.Groups[1].Value == "LOGIN" ? "تسجيل دخول" : "تسجيل خروج",
                        Username = match.Groups[2].Value,
                        Timestamp = match.Groups[3].Value
                    });
                }
            }

            return Ok(ApiResponse<List<UserActivityDto>>.SuccessResponse(activities, "تم استخراج قائمة حركات دخول وخروج المستخدمين بنجاح."));
        }
        catch (IOException ex)
        {
            return StatusCode(500, ApiResponse<object>.FailureResponse(
                ex.Message, "فشل في قراءة حركات المستخدمين بسبب قيود النظام."));
        }
    }
}
