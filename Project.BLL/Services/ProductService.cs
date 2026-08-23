using Project.BLL.DTOs;
using Project.DAL.Entities;
using Project.DAL.Repositories;

namespace Project.BLL.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    private readonly ITaxCalculator _taxCalculator;

    public ProductService(IProductRepository repository, ITaxCalculator taxCalculator)
    {
        _repository = repository;
        _taxCalculator = taxCalculator;
    }

    public async Task<IReadOnlyList<ProductDto>> GetProductsAsync(CancellationToken cancellationToken = default)
    {
        var products = await _repository.GetAllAsync(cancellationToken);
        return products.Select(MapToDto).ToList().AsReadOnly();
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await _repository.GetByIdAsync(id, cancellationToken);
        return product == null ? null : MapToDto(product);
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto), "بيانات المنتج لا يمكن أن تكون فارغة");

        // هنا يتم إنشاء كائن الكيان مع التحقق الداخلي من البيانات (OOP Encapsulation)
        var product = new Product(dto.Name, dto.Price);

        await _repository.AddAsync(product, cancellationToken);

        return MapToDto(product);
    }

    public async Task UpdateProductAsync(int id, UpdateProductDto dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto), "بيانات التعديل لا يمكن أن تكون فارغة");

        var product = await _repository.GetByIdAsync(id, cancellationToken);
        if (product == null)
            throw new KeyNotFoundException($"المنتج ذو المعرف {id} غير موجود");

        // تعديل البيانات عبر كبسلة الكيان (Encapsulated Entity modification)
        product.UpdateDetails(dto.Name, dto.Price);

        await _repository.UpdateAsync(product, cancellationToken);
    }

    public async Task DeleteProductAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await _repository.GetByIdAsync(id, cancellationToken);
        if (product == null)
            throw new KeyNotFoundException($"المنتج ذو المعرف {id} غير موجود");

        await _repository.DeleteAsync(product, cancellationToken);
    }

    private ProductDto MapToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            PriceWithVat = _taxCalculator.CalculateTotalWithTax(product.Price)
        };
    }
}