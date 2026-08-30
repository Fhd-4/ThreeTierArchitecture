using Project.DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.DAL.Interfaces;

public interface ITaskRepository
{
    Task<IEnumerable<ProjectTask>> GetAllAsync(int? projectId, int? status, string? keyword);
    Task<ProjectTask?> GetByIdAsync(int id);
    Task<bool> ProjectExistsAsync(int projectId);
    Task<string?> GetDefaultUserIdAsync();
    Task AddAsync(ProjectTask task);
    void Update(ProjectTask task);
    void Delete(ProjectTask task);
    Task<bool> SaveChangesAsync();
}