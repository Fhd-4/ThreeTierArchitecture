using System.Net;
using System.Text.Json;
using Project.API.Common;

namespace Project.API.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "حدث خطأ غير معالج أثناء معالجة الطلب: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var statusCode = HttpStatusCode.InternalServerError;
        var message = "حدث خطأ داخلي غير متوقع في الخادم";
        var errors = new List<string> { exception.Message };

        // تحديد كود حالة الـ HTTP ورسالة الخطأ بناءً على نوع الاستثناء المرتجع
        switch (exception)
        {
            case ArgumentException:
                statusCode = HttpStatusCode.BadRequest;
                message = "المدخلات المرسلة غير صالحة";
                break;
            case KeyNotFoundException:
                statusCode = HttpStatusCode.NotFound;
                message = "المصدر المطلوب غير موجود";
                break;
            // يمكنك إضافة أنواع استثناءات مخصصة أخرى هنا في المستقبل
        }

        context.Response.StatusCode = (int)statusCode;

        var response = ApiResponse<object>.FailureResponse(errors, message);
        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return context.Response.WriteAsync(jsonResponse);
    }
}
