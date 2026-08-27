using Project.BLL.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.BLL.Services
{
    public interface IProjectService
    {
        Task<IEnumerable<ProjectDetailsDto>> GetProjectsAsync(int? portfolioId, int? programId, string? keyword, string? status);
        Task<ProjectDetailsDto?> GetProjectByIdAsync(int id);
        Task<ProjectDetailsDto> CreateProjectAsync(CreateProjectDto dto, string? userId);
        Task<bool> UpdateProjectAsync(int id, UpdateProjectDto dto);
        Task<bool> DeleteProjectAsync(int id);
    }
}
