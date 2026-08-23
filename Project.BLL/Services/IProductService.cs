using Project.BLL.DTOs;

namespace Project.BLL.Services;

public interface IProductService
{
    IReadOnlyList<ProductDto> GetProducts();
}