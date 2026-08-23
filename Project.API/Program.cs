using Microsoft.EntityFrameworkCore;
using Project.BLL.Services;
using Project.DAL.Data;
using Project.DAL.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 1. تسجيل قاعدة البيانات (SQL Server Database)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. تسجيل الـ Repositories والـ Services
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ITaxCalculator, SaudiVatCalculator>();
builder.Services.AddScoped<IProductService, ProductService>();

var app = builder.Build();

// تسجيل معالج الأخطاء المركزي (Global Exception Handler) في بداية خط المعالجة
app.UseMiddleware<Project.API.Middlewares.ExceptionHandlingMiddleware>();

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
app.MapControllers();

app.Run();