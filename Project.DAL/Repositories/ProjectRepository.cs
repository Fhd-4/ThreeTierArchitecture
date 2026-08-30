using Microsoft.EntityFrameworkCore;
using Project.DAL.Data;
using Project.DAL.Entities;
using Project.DAL.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.DAL.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly AppDbContext _context;

    public ProjectRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Project.DAL.Entities.Project>> GetAllAsync(int? portfolioId, int? programId, string? keyword, int? status)
    {
        var query = _context.Projects
            .Include(p => p.Portfolio)
            .Include(p => p.Program)
            .Include(p => p.Tasks)
            .Include(p => p.ProjectMembers)
            .AsQueryable();

        if (portfolioId.HasValue)
            query = query.Where(p => p.PortfolioId == portfolioId.Value);

        if (programId.HasValue)
            query = query.Where(p => p.ProgramId == programId.Value);

        if (status.HasValue)
            query = query.Where(p => p.Status == status.Value);

        if (!string.IsNullOrEmpty(keyword))
            query = query.Where(p => p.Name.Contains(keyword) || (p.Description != null && p.Description.Contains(keyword)));

        return await query.ToListAsync();
    }

    public async Task<Project.DAL.Entities.Project?> GetByIdAsync(int id)
    {
        return await _context.Projects
            .Include(p => p.Portfolio)
            .Include(p => p.Program)
            .Include(p => p.Tasks)
            .Include(p => p.ProjectMembers)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task AddAsync(Project.DAL.Entities.Project project)
    {
        await _context.Projects.AddAsync(project);
    }

    public void Update(Project.DAL.Entities.Project project)
    {
        _context.Projects.Update(project);
    }

    public void Delete(Project.DAL.Entities.Project project)
    {
        _context.Projects.Remove(project);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> PortfolioExistsAsync(int portfolioId)
    {
        return await _context.Portfolios.AnyAsync(p => p.Id == portfolioId);
    }

    public async Task<bool> ProgramExistsAsync(int programId)
    {
        return await _context.Programs.AnyAsync(p => p.Id == programId);
    }
}
