using Project.DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.DAL.Interfaces;

public interface ITaskRepository
{
    Task<IEnumerable<ProjectTask>> GetAllTasksAsync(int? projectId, int? status, string? keyword);
    Task<ProjectTask?> GetByIdAsync(int id);
    Task AddAsync(ProjectTask task);
    void Update(ProjectTask task);
    void Delete(ProjectTask task);
    Task<bool> SaveChangesAsync();
    Task<bool> ProjectExistsAsync(int projectId);
}
