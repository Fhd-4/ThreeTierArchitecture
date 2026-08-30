using Microsoft.EntityFrameworkCore;
using Project.DAL.Data;
using Project.DAL.Entities;
using Project.DAL.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.DAL.Repositories;

public class MeetingRepository : IMeetingRepository
{
    private readonly AppDbContext _context;

    public MeetingRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProjectMeeting>> GetAllAsync(int? projectId, string? keyword)
    {
        var query = _context.Meetings
            .Include(m => m.Project)
            .AsQueryable();

        if (projectId.HasValue)
            query = query.Where(m => m.ProjectId == projectId.Value);

        if (!string.IsNullOrEmpty(keyword))
            query = query.Where(m => m.Title.Contains(keyword) || (m.Description != null && m.Description.Contains(keyword)));

        return await query.ToListAsync();
    }

    public async Task<ProjectMeeting?> GetByIdAsync(int id)
    {
        return await _context.Meetings
            .Include(m => m.Project)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task AddAsync(ProjectMeeting meeting)
    {
        await _context.Meetings.AddAsync(meeting);
    }

    public void Update(ProjectMeeting meeting)
    {
        _context.Meetings.Update(meeting);
    }

    public void Delete(ProjectMeeting meeting)
    {
        _context.Meetings.Remove(meeting);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> ProjectExistsAsync(int projectId)
    {
        return await _context.Projects.AnyAsync(p => p.Id == projectId);
    }
}
