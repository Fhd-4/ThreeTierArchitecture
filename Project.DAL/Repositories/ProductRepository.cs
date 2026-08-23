using Project.DAL.Entities;

namespace Project.DAL.Repositories;

public interface IProductRepository
{
    List<Product> GetAll();
}

public class ProductRepository : IProductRepository
{
    // بيانات وهمية مؤقتة تحاكي الداتا بيس
    public List<Product> GetAll()
    {
        return new List<Product>
        {
            new Product { Id = 1, Name = "Laptop", Price = 3500 },
            new Product { Id = 2, Name = "Mouse", Price = 150 }
        };
    }
}