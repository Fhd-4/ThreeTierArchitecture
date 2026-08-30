using Microsoft.EntityFrameworkCore;
using Project.DAL.Data;
using Project.DAL.Entities;
using Project.DAL.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.DAL.Repositories;

public class PortfolioRepository : IPortfolioRepository
{
    private readonly AppDbContext _context;

    public PortfolioRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Portfolio>> GetAllAsync()
    {
        return await _context.Portfolios
            .Include(p => p.Owner)
            .Include(p => p.Programs)
            .Include(p => p.Projects)
            .ToListAsync();
    }

    public async Task<Portfolio?> GetByIdAsync(int id)
    {
        return await _context.Portfolios
            .Include(p => p.Owner)
            .Include(p => p.Programs)
            .Include(p => p.Projects)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task AddAsync(Portfolio portfolio)
    {
        await _context.Portfolios.AddAsync(portfolio);
    }

    public void Update(Portfolio portfolio)
    {
        _context.Portfolios.Update(portfolio);
    }

    public void Delete(Portfolio portfolio)
    {
        _context.Portfolios.Remove(portfolio);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<IEnumerable<ApplicationUser>> GetUsersForTestAsync()
    {
        return await _context.Users.ToListAsync();
    }

    public async Task<object> GetStatsAsync()
    {
        var portfolios = await _context.Portfolios
            .Include(p => p.Projects)
            .Include(p => p.Programs)
            .ToListAsync();

        var totalBudget = portfolios.Sum(p => p.Budget);
        var totalProjects = portfolios.Sum(p => p.Projects.Count);
        var totalPrograms = portfolios.Sum(p => p.Programs.Count);

        return new
        {
            TotalPortfolios = portfolios.Count,
            TotalPrograms = totalPrograms,
            TotalProjects = totalProjects,
            TotalBudget = totalBudget
        };
    }
}
