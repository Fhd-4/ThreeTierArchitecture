using Project.DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.DAL.Interfaces;

public interface IProjectRepository
{
    Task<IEnumerable<Project.DAL.Entities.Project>> GetAllAsync(int? portfolioId, int? programId, string? keyword, int? status);
    Task<Project.DAL.Entities.Project?> GetByIdAsync(int id);
    Task AddAsync(Project.DAL.Entities.Project project);
    void Update(Project.DAL.Entities.Project project);
    void Delete(Project.DAL.Entities.Project project);
    Task<bool> SaveChangesAsync();
    Task<bool> PortfolioExistsAsync(int portfolioId);
    Task<bool> ProgramExistsAsync(int programId);
}
