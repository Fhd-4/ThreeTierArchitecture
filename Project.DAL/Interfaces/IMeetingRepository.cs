using Project.DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.DAL.Interfaces;

public interface IMeetingRepository
{
    Task<IEnumerable<ProjectMeeting>> GetAllAsync(int? projectId, string? keyword);
    Task<ProjectMeeting?> GetByIdAsync(int id);
    Task AddAsync(ProjectMeeting meeting);
    void Update(ProjectMeeting meeting);
    void Delete(ProjectMeeting meeting);
    Task<bool> SaveChangesAsync();
    Task<bool> ProjectExistsAsync(int projectId);
}
