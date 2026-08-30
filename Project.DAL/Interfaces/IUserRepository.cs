using Project.DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.DAL.Interfaces;

public interface IUserRepository
{
    Task<(bool hasPortfolios, bool hasPrograms, bool hasProjects, bool hasTasks, bool hasChangeRequests)> CheckUserDependenciesAsync(string userId);
    Task CleanUserDependenciesAsync(string userId);
    Task AssociateUserWithEntitiesAsync(string userId, int? portfolioId, int? programId, int? projectId);
    Task<List<Portfolio>> GetPortfoliosByOwnerIdAsync(string ownerId);
    Task<List<ProjectProgram>> GetProgramsByManagerIdAsync(string managerId);
    Task<List<Project.DAL.Entities.Project>> GetProjectsByManagerIdAsync(string managerId);
}
