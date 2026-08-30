using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Project.API.Common;
using Project.BLL.DTOs;
using Project.BLL.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Project.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PortfoliosController : ControllerBase
{
    private readonly IPortfolioService _portfolioService;

    public PortfoliosController(IPortfolioService portfolioService)
    {
        _portfolioService = portfolioService;
    }

    // 1. Get all portfolios
    [HttpGet]
    public async Task<IActionResult> GetPortfolios()
    {
        var portfolios = await _portfolioService.GetPortfoliosAsync();
        return Ok(portfolios); // Return raw list for Angular frontend compatibility
    }

    // 2. Get single portfolio by ID
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetPortfolio(int id)
    {
        var portfolio = await _portfolioService.GetPortfolioByIdAsync(id);
        if (portfolio == null)
        {
            return NotFound();
        }
        return Ok(portfolio);
    }

    // 3. Create a portfolio
    [HttpPost]
    public async Task<IActionResult> CreatePortfolio([FromBody] CreatePortfolioDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "default-user-id";
        var createdPortfolio = await _portfolioService.CreatePortfolioAsync(dto, userId);

        return StatusCode(201, createdPortfolio);
    }

    // 4. Update a portfolio
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdatePortfolio(int id, [FromBody] UpdatePortfolioDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var success = await _portfolioService.UpdatePortfolioAsync(id, dto);
        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    // 5. Delete a portfolio
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeletePortfolio(int id)
    {
        var success = await _portfolioService.DeletePortfolioAsync(id);
        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    // 6. Test Users DB endpoint
    [HttpGet("test-users-db")]
    public async Task<IActionResult> TestUsersDb()
    {
        var users = await _portfolioService.GetUsersForTestAsync();
        return Ok(users);
    }

    // 7. Get Portfolio stats
    [HttpGet("stats")]
    public async Task<IActionResult> GetPortfolioStats()
    {
        var stats = await _portfolioService.GetStatsAsync();
        return Ok(stats);
    }

    // 8. Upload files for portfolios
    [HttpPost("upload")]
    public async Task<IActionResult> UploadFiles(List<IFormFile> files)
    {
        if (files == null || files.Count == 0)
        {
            return BadRequest("No files uploaded.");
        }

        var uploadedFilesList = new List<object>();
        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        foreach (var file in files)
        {
            if (file.Length > 0)
            {
                var originalName = file.FileName;
                var uniqueName = Guid.NewGuid().ToString() + Path.GetExtension(originalName);
                var filePath = Path.Combine(uploadsFolder, uniqueName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var sizeInMb = (file.Length / (1024.0 * 1024.0)).ToString("F1") + " MB";
                var ext = Path.GetExtension(originalName).TrimStart('.').ToLower();

                uploadedFilesList.Add(new
                {
                    name = originalName,
                    path = "/uploads/" + uniqueName,
                    size = sizeInMb,
                    type = ext
                });
            }
        }

        return Ok(uploadedFilesList);
    }
}
