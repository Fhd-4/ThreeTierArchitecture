using Project.DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.DAL.Interfaces;

public interface IProgramRepository
{
    Task<IEnumerable<ProjectProgram>> GetAllAsync(int? portfolioId, string? keyword, int? status);
    Task<ProjectProgram?> GetByIdAsync(int id);
    Task<ProjectProgram?> FindAsync(int id);
    Task AddAsync(ProjectProgram program);
    void Delete(ProjectProgram program);
    Task<bool> SaveChangesAsync();
}