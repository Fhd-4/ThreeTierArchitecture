using Microsoft.EntityFrameworkCore;
using Project.DAL.Data;
using Project.DAL.Entities;
using Project.DAL.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.DAL.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _context;

    public TaskRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProjectTask>> GetAllTasksAsync(int? projectId, int? status, string? keyword)
    {
        var query = _context.Tasks
            .Include(t => t.Project)
            .Include(t => t.Assignee)
            .AsQueryable();

        if (projectId.HasValue)
            query = query.Where(t => t.ProjectId == projectId.Value);

        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);

        if (!string.IsNullOrEmpty(keyword))
            query = query.Where(t => t.Title.Contains(keyword) || (t.Description != null && t.Description.Contains(keyword)));

        return await query.ToListAsync();
    }

    public async Task<ProjectTask?> GetByIdAsync(int id)
    {
        return await _context.Tasks
            .Include(t => t.Project)
            .Include(t => t.Assignee)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task AddAsync(ProjectTask task)
    {
        await _context.Tasks.AddAsync(task);
    }

    public void Update(ProjectTask task)
    {
        _context.Tasks.Update(task);
    }

    public void Delete(ProjectTask task)
    {
        _context.Tasks.Remove(task);
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
