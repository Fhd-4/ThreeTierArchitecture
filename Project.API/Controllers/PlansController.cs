using Microsoft.AspNetCore.Mvc;
using Project.BLL.Services;
using Project.DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PlansController : ControllerBase
{
    private readonly IPlanService _planService;

    public PlansController(IPlanService planService)
    {
        _planService = planService;
    }

    [HttpGet("all")]
    public async Task<ActionResult<IEnumerable<Plan>>> GetAllPlans()
    {
        var plans = await _planService.GetAllPlansAsync();
        return Ok(plans);
    }

    [HttpGet("details/{id}")]
    public async Task<ActionResult<Plan>> GetPlanDetails(int id)
    {
        var plan = await _planService.GetPlanByIdAsync(id);
        if (plan == null)
            return NotFound();

        return Ok(plan);
    }

    [HttpPost("create")]
    public async Task<ActionResult<Plan>> CreatePlan([FromBody] Plan plan)
    {
        var createdPlan = await _planService.CreatePlanAsync(plan);
        return CreatedAtAction(nameof(GetPlanDetails), new { id = createdPlan.Id }, createdPlan);
    }

    [HttpPut("update/{id}")]
    public async Task<IActionResult> UpdatePlan(int id, [FromBody] Plan plan)
    {
        if (id != plan.Id)
            return BadRequest();

        var (success, notFound) = await _planService.UpdatePlanAsync(id, plan);
        if (notFound)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeletePlan(int id)
    {
        var deleted = await _planService.DeletePlanAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}