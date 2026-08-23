using Project.BLL.Services;
using Project.DAL.Repositories;

var builder = WebApplication.CreateBuilder(args);

// 1. تفعيل نظام الـ Controllers وتوثيق OpenAPI
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

// 2. تسجيل خدمات الطبقات (Dependency Injection)
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

var app = builder.Build();

// 3. إعدادات بيئة التطوير
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();



