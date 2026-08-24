using MongoDB.Driver;
using Project.API.Middlewares;
using Project.BLL.Services;
using Project.DAL.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 1. تسجيل عميل MongoDB (NoSQL Database)
builder.Services.AddSingleton<IMongoClient>(sp => 
    new MongoClient(builder.Configuration.GetConnectionString("MongoConnection")));

// 2. تسجيل الـ Repositories والـ Services
// قمنا فقط بتغيير الفئة الملموسة لتكون MongoProductRepository بدلاً من ProductRepository
builder.Services.AddScoped<IProductRepository, MongoProductRepository>();
builder.Services.AddScoped<ITaxCalculator, SaudiVatCalculator>();
builder.Services.AddScoped<IProductService, ProductService>();

var app = builder.Build();

// تسجيل معالج الأخطاء المركزي (Global Exception Handler) في بداية خط المعالجة
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();