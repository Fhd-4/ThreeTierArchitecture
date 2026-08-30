using Microsoft.EntityFrameworkCore;
using Project.DAL.Data;
using Project.DAL.Entities;
using Project.DAL.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.DAL.Repositories;

public class PlanRepository : IPlanRepository
{
    private readonly AppDbContext _context;

    public PlanRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Plan>> GetAllAsync()
    {
        return await _context.Plans
            .Include(p => p.Milestones)
            .Include(p => p.Deliverables)
            .ToListAsync();
    }

    public async Task<Plan?> GetByIdAsync(int id)
    {
        return await _context.Plans
            .Include(p => p.Milestones)
            .Include(p => p.Deliverables)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Plans.AnyAsync(p => p.Id == id);
    }

    public async Task AddAsync(Plan plan)
    {
        await _context.Plans.AddAsync(plan);
    }

    public void Update(Plan plan)
    {
        _context.Plans.Update(plan);
    }

    public void Delete(Plan plan)
    {
        _context.Plans.Remove(plan);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}