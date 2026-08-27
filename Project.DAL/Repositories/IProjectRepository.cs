using Project.DAL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.DAL.Repositories
{
    public interface IProjectRepository
    {
        Task<IEnumerable<Project.DAL.Models.Project>> GetAllProjectsAsync(int? portfolioId, int? programId, string? keyword, int? status);
        Task<Project.DAL.Models.Project?> GetByIdAsync(int id);
        Task AddAsync(Project.DAL.Models.Project project);
        void Update(Project.DAL.Models.Project project);
        void Delete(Project.DAL.Models.Project project);
        Task<bool> SaveChangesAsync();
    }
}
