using Project.DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.DAL.Interfaces;

public interface IPortfolioRepository
{
    Task<IEnumerable<Portfolio>> GetAllAsync();
    Task<Portfolio?> GetByIdAsync(int id);
    Task AddAsync(Portfolio portfolio);
    void Update(Portfolio portfolio);
    void Delete(Portfolio portfolio);
    Task<bool> SaveChangesAsync();
}
