using Project.DAL.Entities;
using Project.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.BLL.Services;

public class PlanService : IPlanService
{
    private readonly IPlanRepository _repo;

    public PlanService(IPlanRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<Plan>> GetAllPlansAsync()
    {
        return await _repo.GetAllAsync();
    }

    public async Task<Plan?> GetPlanByIdAsync(int id)
    {
        return await _repo.GetByIdAsync(id);
    }

    public async Task<Plan> CreatePlanAsync(Plan plan)
    {
        plan.LastUpdated = DateTime.UtcNow;
        await _repo.AddAsync(plan);
        await _repo.SaveChangesAsync();
        return plan;
    }

    public async Task<(bool Success, bool NotFound)> UpdatePlanAsync(int id, Plan plan)
    {
        var exists = await _repo.ExistsAsync(id);
        if (!exists)
            return (false, true);

        plan.LastUpdated = DateTime.UtcNow;
        _repo.Update(plan);
        var success = await _repo.SaveChangesAsync();
        return (success, false);
    }

    public async Task<bool> DeletePlanAsync(int id)
    {
        var plan = await _repo.GetByIdAsync(id);
        if (plan == null)
            return false;

        _repo.Delete(plan);
        return await _repo.SaveChangesAsync();
    }
}