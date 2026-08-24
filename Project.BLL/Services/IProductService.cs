using Project.BLL.DTOs;

namespace Project.BLL.Services;

public interface IProductService
{
    Task<IReadOnlyList<ProductDto>> GetProductsAsync(CancellationToken cancellationToken = default);
    Task<ProductDto?> GetProductByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<ProductDto> CreateProductAsync(CreateProductDto dto, CancellationToken cancellationToken = default);
    Task UpdateProductAsync(string id, UpdateProductDto dto, CancellationToken cancellationToken = default);
    Task DeleteProductAsync(string id, CancellationToken cancellationToken = default);
}