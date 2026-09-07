using Project.DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.DAL.Interfaces;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllAsync(string? keyword);
    Task<Category?> GetByIdAsync(int id);
    Task AddAsync(Category category);
    void Update(Category category);
    void Delete(Category category);
    Task<bool> SaveChangesAsync();
}
