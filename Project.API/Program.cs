using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Project.API.Middlewares;
using Project.API.Hubs;
using Project.DAL.Data;
using Project.DAL.Models;
using Project.DAL.Repositories;
using Project.BLL.Services;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// تهيئة وإعداد Serilog لقراءة إعدادات السجلات من appsettings.json
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();

// 1. تسجيل قاعدة البيانات (SQL Server Database)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. تسجيل خدمات الـ Identity لإدارة المستخدمين والصلاحيات
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// 3. إعداد الـ JWT لتوثيق الجلسات وحماية الـ APIs
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key is missing in settings.");

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };

        // تفعيل استرداد الـ Token للـ SignalR ChatHub من الاستعلام
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chathub"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// 4. تسجيل المستودعات والخدمات (Dependency Injection)
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IProjectService, ProjectService>();

// 5. إضافة وتكوين سياسة الـ CORS للسماح لتطبيق الأنجولر وتفعيل الـ SignalR Credentials
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200") // رابط تطبيق الأنجولر المحلي
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // مطلوبة للـ SignalR
    });
});

// 6. تكوين Swagger لدعم ترويسات الـ JWT Bearer
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Description = "ادخل الـ Token فقط بدون كلمة Bearer"
    });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        });
});

var app = builder.Build();

// تسجيل معالج الأخطاء المركزي (Global Exception Handler) في بداية خط المعالجة
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapGet("/", context =>
{
    context.Response.Redirect("/swagger/index.html", permanent: false);
    return Task.CompletedTask;
});

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

// تفعيل سياسة الـ CORS قبل التوثيق والصلاحيات
app.UseCors("AllowAngularApp");

// تمكين استعراض الملفات المرفوعة مباشرة
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/chathub");

// تهيئة وتحديث قاعدة البيانات تلقائياً وتطبيق الهجرات (Migrations)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.Migrate();

        // Seeding Data لتأهيل وتفعيل حالة المستخدمين وتواريخهم الافتراضية
        var usersList = await dbContext.Users.ToListAsync();
        foreach (var u in usersList)
        {
            u.IsActive = true;
            u.PhoneNumberConfirmed = true;
            if (u.CreatedDate == default)
            {
                u.CreatedDate = new DateTime(2026, 1, 1);
            }
            if (string.IsNullOrEmpty(u.Email))
            {
                u.Email = $"{u.UserName?.ToLower()}@example.com";
                u.NormalizedEmail = u.Email.ToUpper();
            }
            if (string.IsNullOrEmpty(u.NameEn))
            {
                u.NameEn = u.UserName;
            }
            if (string.IsNullOrEmpty(u.NameAr))
            {
                u.NameAr = u.UserName;
            }
        }
        await dbContext.SaveChangesAsync();
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "حدث خطأ غير متوقع أثناء تهيئة وتحديث قاعدة البيانات!");
    }
}

try
{
    Log.Information("جاري بدء تشغيل خادم ProjectManagement المطور...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "تعطل إقلاع الخادم بشكل غير متوقع!");
}
finally
{
    Log.CloseAndFlush();
}
