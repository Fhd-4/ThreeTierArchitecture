using Microsoft.EntityFrameworkCore;
using Project.DAL.Data;
using Project.DAL.Entities;
using Project.DAL.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.DAL.Repositories;

public class ProgramRepository : IProgramRepository
{
    private readonly AppDbContext _context;

    public ProgramRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProjectProgram>> GetAllAsync(int? portfolioId, string? keyword, int? status)
    {
        var query = _context.Programs
            .Include(p => p.Portfolio)
            .Include(p => p.Manager)
            .Include(p => p.Projects)
                .ThenInclude(proj => proj.Tasks)
            .AsQueryable();

        if (portfolioId.HasValue)
            query = query.Where(p => p.PortfolioId == portfolioId.Value);

        if (!string.IsNullOrEmpty(keyword))
            query = query.Where(p => p.Name.Contains(keyword) || (p.Description != null && p.Description.Contains(keyword)));

        if (status.HasValue)
            query = query.Where(p => p.Status == status.Value);

        return await query.ToListAsync();
    }

    public async Task<ProjectProgram?> GetByIdAsync(int id)
    {
        return await _context.Programs
            .Include(p => p.Portfolio)
            .Include(p => p.Manager)
            .Include(p => p.Projects)
                .ThenInclude(proj => proj.Tasks)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<ProjectProgram?> FindAsync(int id)
    {
        return await _context.Programs.FindAsync(id);
    }

    public async Task AddAsync(ProjectProgram program)
    {
        await _context.Programs.AddAsync(program);
    }

    public void Delete(ProjectProgram program)
    {
        _context.Programs.Remove(program);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}