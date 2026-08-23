using Project.DAL.Entities;

namespace Project.DAL.Repositories;

public interface IProductRepository
{
    IReadOnlyList<Product> GetAll();
    Product? GetById(int id);
}