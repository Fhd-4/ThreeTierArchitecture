using Project.DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.DAL.Interfaces;

public interface IPlanRepository
{
    Task<IEnumerable<Plan>> GetAllAsync();
    Task<Plan?> GetByIdAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task AddAsync(Plan plan);
    void Update(Plan plan);
    void Delete(Plan plan);
    Task<bool> SaveChangesAsync();
}