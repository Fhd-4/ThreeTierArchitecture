using Project.DAL.Entities;

namespace Project.DAL.Repositories;

public interface IReadOnlyRepository<T> where T : BaseEntity
{
    IReadOnlyList<T> GetAll();
    T? GetById(int id);
}

public interface IProductRepository : IReadOnlyRepository<Product>
{
}

public class ProductRepository : IProductRepository
{
    private readonly List<Product> _products = new()
    {
        new Product(1, "Laptop", 3500),
        new Product(2, "Mouse", 150)
    };

    public IReadOnlyList<Product> GetAll() => _products.AsReadOnly();

    public Product? GetById(int id) => _products.FirstOrDefault(p => p.Id == id);
}