using Project.BLL.DTOs;
using Project.DAL.Repositories;

namespace Project.BLL.Services;

public interface IProductService
{
    List<ProductDto> GetProducts();
}

public class ProductService : IProductService
{
    private readonly IProductRepository _repo;

    public ProductService(IProductRepository repo)
    {
        _repo = repo;
    }

    public List<ProductDto> GetProducts()
    {
        // 1. جلب البيانات من طبقة البيانات (DAL)
        var products = _repo.GetAll();

        // 2. تطبيق منطق العمل (BLL): حساب ضريبة 15% وتحويلها لـ DTO
        return products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            PriceWithVat = p.Price * 1.15m
        }).ToList();
    }
}