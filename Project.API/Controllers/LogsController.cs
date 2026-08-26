using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Project.API.Common;
using System.IO;

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

    // دالة (Function) لقراءة سجلات النظام الحالية وعرضها كـ API Response
    [HttpGet]
    public IActionResult GetLogs()
    {
        // استخدام CultureInfo.InvariantCulture لضمان استخدام التقويم الميلادي (Gregorian)
        // وتجنب استخدام التقويم الهجري إذا كانت ثقافة السيرفر عربية/هجرية
        var today = DateTime.UtcNow.ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
        var logFileName = $"log-{today}.txt";
        
        // استخدام ContentRootPath لضمان الوصول للمسار الصحيح بغض النظر عن طريقة تشغيل المشروع
        var logFilePath = Path.Combine(_env.ContentRootPath, "logs", logFileName);

        if (!System.IO.File.Exists(logFilePath))
        {
            return Ok(ApiResponse<List<string>>.SuccessResponse(new List<string>(), $"ملف اللوج غير موجود في المسار: {logFilePath}"));
        }

        try
        {
            // نفتح الملف باستخدام FileShare.ReadWrite لتجنب حدوث خطأ قفل الملف أثناء كتابة السيرفر فيه
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
}
