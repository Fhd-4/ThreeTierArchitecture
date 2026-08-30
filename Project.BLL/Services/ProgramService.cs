using Project.BLL.DTOs;
using Project.DAL.Entities;
using Project.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.BLL.Services;

public class ProgramService : IProgramService
{
    private readonly IProgramRepository _repo;

    public ProgramService(IProgramRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<ProgramDetailsDto>> GetProgramsAsync(int? portfolioId, string? keyword, int? status)
    {
        var programs = await _repo.GetAllAsync(portfolioId, keyword, status);
        return programs.Select(MapToDetailsDto).ToList();
    }

    public async Task<ProgramDetailsDto?> GetProgramByIdAsync(int id)
    {
        var program = await _repo.GetByIdAsync(id);
        if (program == null) return null;
        return MapToDetailsDto(program);
    }

    public async Task<ProjectProgram> CreateProgramAsync(CreateProgramDto dto)
    {
        var program = new ProjectProgram
        {
            Name = dto.Name,
            Description = dto.Description,
            Budget = dto.Budget,
            Status = dto.Status,
            PortfolioId = dto.PortfolioId,
            SponsorName = dto.SponsorName,
            ManagerId = dto.ManagerId,
            CreatedDate = DateTime.UtcNow,
            ProgressPercentage = 0,
            AttachedDocumentUrls = dto.AttachedUrls != null ? string.Join(",", dto.AttachedUrls) : null
        };

        await _repo.AddAsync(program);
        await _repo.SaveChangesAsync();
        return program;
    }

    public async Task<ProjectProgram?> UpdateProgramAsync(int id, UpdateProgramDto dto)
    {
        var program = await _repo.FindAsync(id);
        if (program == null) return null;

        program.Name = dto.Name;
        program.Description = dto.Description;
        program.Budget = dto.Budget;
        program.Status = dto.Status;
        program.ProgressPercentage = dto.ProgressPercentage;
        program.SponsorName = dto.SponsorName;
        program.ManagerId = dto.ManagerId;
        program.AttachedDocumentUrls = dto.AttachedUrls != null ? string.Join(",", dto.AttachedUrls) : null;

        await _repo.SaveChangesAsync();
        return program;
    }

    public async Task<(bool Success, string? ErrorMessage)> DeleteProgramAsync(int id)
    {
        var program = await _repo.GetByIdAsync(id);
        if (program == null)
            return (false, "Not Found");

        if (program.Projects != null && program.Projects.Any())
            return (false, "Cannot delete a program that currently contains active projects.");

        _repo.Delete(program);
        await _repo.SaveChangesAsync();
        return (true, null);
    }

    private ProgramDetailsDto MapToDetailsDto(ProjectProgram p)
    {
        return new ProgramDetailsDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Budget = p.Budget,
            Status = p.Status,
            ProgressPercentage = p.ProgressPercentage,
            SponsorName = p.SponsorName,
            ManagerName = p.Manager != null ? p.Manager.UserName : "Not Assigned",
            PortfolioName = p.Portfolio != null ? p.Portfolio.Name : "N/A",
            PortfolioId = p.PortfolioId,
            CreatedDate = p.CreatedDate,
            AttachedDocumentUrls = !string.IsNullOrEmpty(p.AttachedDocumentUrls)
                ? p.AttachedDocumentUrls.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                : new List<string>(),
            ProjectsCount = p.Projects != null ? p.Projects.Count : 0,
            TasksCount = p.Projects != null ? p.Projects.Where(proj => proj.Tasks != null).SelectMany(proj => proj.Tasks).Count() : 0
        };
    }
}