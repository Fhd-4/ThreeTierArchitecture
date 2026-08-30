using Microsoft.EntityFrameworkCore;
using Project.DAL.Data;
using Project.DAL.Entities;
using Project.DAL.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.DAL.Repositories;

public class ChangeRequestRepository : IChangeRequestRepository
{
    private readonly AppDbContext _context;

    public ChangeRequestRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ChangeRequest>> GetAllAsync(int? projectId, string? keyword)
    {
        var query = _context.ChangeRequests
            .Include(cr => cr.Project)
            .Include(cr => cr.RequestedBy)
            .Include(cr => cr.ApprovedBy)
            .AsQueryable();

        if (projectId.HasValue)
        {
            query = query.Where(cr => cr.ProjectId == projectId.Value);
        }

        if (!string.IsNullOrEmpty(keyword))
        {
            query = query.Where(cr => cr.Title.Contains(keyword) || cr.Description.Contains(keyword) || cr.Reason.Contains(keyword));
        }

        return await query.ToListAsync();
    }

    public async Task<ChangeRequest?> GetByIdAsync(int id)
    {
        return await _context.ChangeRequests
            .Include(cr => cr.Project)
            .Include(cr => cr.RequestedBy)
            .Include(cr => cr.ApprovedBy)
            .FirstOrDefaultAsync(cr => cr.Id == id);
    }

    public async Task AddAsync(ChangeRequest changeRequest)
    {
        await _context.ChangeRequests.AddAsync(changeRequest);
    }

    public void Update(ChangeRequest changeRequest)
    {
        _context.ChangeRequests.Update(changeRequest);
    }

    public void Delete(ChangeRequest changeRequest)
    {
        _context.ChangeRequests.Remove(changeRequest);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<IEnumerable<ChangeRequestComment>> GetCommentsForRequestAsync(int requestId)
    {
        return await _context.ChangeRequestComments
            .Where(c => c.ChangeRequestId == requestId)
            .OrderBy(c => c.CreatedDate)
            .ToListAsync();
    }

    public async Task AddCommentAsync(ChangeRequestComment comment)
    {
        await _context.ChangeRequestComments.AddAsync(comment);
    }

    public async Task<bool> ProjectExistsAsync(int projectId)
    {
        return await _context.Projects.AnyAsync(p => p.Id == projectId);
    }

    public async Task<ApplicationUser?> GetUserByIdAsync(string userId)
    {
        return await _context.Users.FindAsync(userId);
    }

    public async Task<ApplicationUser?> GetFirstUserAsync()
    {
        return await _context.Users.FirstOrDefaultAsync();
    }
}
