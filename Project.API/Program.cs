using Microsoft.EntityFrameworkCore;
using Project.API.Middlewares;
using Project.DAL.Data;
using Project.DAL.Interfaces;
using Project.DAL.Repositories;
using Project.BLL.Services;
using Serilog;
using Microsoft.AspNetCore.Identity;
using Project.DAL.Entities;

var builder = WebApplication.CreateBuilder(args);

// تهيئة وإعداد Serilog لقراءة إعدادات السجلات من appsettings.json
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 1. تسجيل قاعدة البيانات (SQL Server Database)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();
// تسجيل خدمات المستودعات والمنطق (Repositories & Services)
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IPortfolioRepository, PortfolioRepository>();
builder.Services.AddScoped<IPortfolioService, PortfolioService>();
builder.Services.AddScoped<IProgramRepository, ProgramRepository>();
builder.Services.AddScoped<IProgramService, ProgramService>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IMeetingRepository, MeetingRepository>();
builder.Services.AddScoped<IMeetingService, MeetingService>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IPlanRepository, PlanRepository>();
builder.Services.AddScoped<IPlanService, PlanService>();

// 2. إضافة وتكوين سياسة الـ CORS للسماح للأنجولر بالاتصال بالباك إند
builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularAppPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:4200") // عنوان تطبيق الأنجولر
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// تسجيل معالج الأخطاء المركزي (Global Exception Handler) في بداية خط المعالجة
app.UseMiddleware<ExceptionHandlingMiddleware>();

// تفعيل سياسة الـ CORS التي تم تكوينها سابقاً
app.UseCors("AngularAppPolicy");

// تهيئة وتحديث قاعدة البيانات تلقائياً وتطبيق الهجرات (Migrations) عند تشغيل التطبيق
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

try
{
    Log.Information("جاري بدء تشغيل الخادم والباك إند...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "فشل إقلاع الخادم بشكل غير متوقع!");
}
finally
{
    Log.CloseAndFlush();
}