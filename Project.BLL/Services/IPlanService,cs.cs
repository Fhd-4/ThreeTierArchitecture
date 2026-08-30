using Project.DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.BLL.Services;

public interface IPlanService
{
    Task<IEnumerable<Plan>> GetAllPlansAsync();
    Task<Plan?> GetPlanByIdAsync(int id);
    Task<Plan> CreatePlanAsync(Plan plan);
    Task<(bool Success, bool NotFound)> UpdatePlanAsync(int id, Plan plan);
    Task<bool> DeletePlanAsync(int id);
}