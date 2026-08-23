using Project.BLL.DTOs;
using Project.DAL.Repositories;

namespace Project.BLL.Services;

public interface IProductService
{
    IReadOnlyList<ProductDto> GetProducts();
}

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    private readonly ITaxCalculator _taxCalculator;

    public ProductService(IProductRepository repository, ITaxCalculator taxCalculator)
    {
        _repository = repository;
        _taxCalculator = taxCalculator;
    }

    public IReadOnlyList<ProductDto> GetProducts()
    {
        var products = _repository.GetAll();

        // 2. تطبيق منطق العمل (BLL): حساب ضريبة 15% وتحويلها لـ DTO
        return products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            PriceWithVat = _taxCalculator.CalculateTotalWithTax(p.Price)
        }).ToList().AsReadOnly();
    }
}