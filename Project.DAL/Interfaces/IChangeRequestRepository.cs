using Project.DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.DAL.Interfaces;

public interface IChangeRequestRepository
{
    Task<IEnumerable<ChangeRequest>> GetAllAsync(int? projectId, string? keyword);
    Task<ChangeRequest?> GetByIdAsync(int id);
    Task AddAsync(ChangeRequest changeRequest);
    void Update(ChangeRequest changeRequest);
    void Delete(ChangeRequest changeRequest);
    Task<bool> SaveChangesAsync();
    
    // Comments
    Task<IEnumerable<ChangeRequestComment>> GetCommentsForRequestAsync(int requestId);
    Task AddCommentAsync(ChangeRequestComment comment);

    // Helpers
    Task<bool> ProjectExistsAsync(int projectId);
    Task<ApplicationUser?> GetUserByIdAsync(string userId);
    Task<ApplicationUser?> GetFirstUserAsync();
}
