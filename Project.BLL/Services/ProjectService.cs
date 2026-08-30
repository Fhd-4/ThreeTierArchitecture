using Project.BLL.DTOs;
using Project.DAL.Entities;
using Project.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.BLL.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _repo;

    public ProjectService(IProjectRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<ProjectDetailsDto>> GetProjectsAsync(int? portfolioId, int? programId, string? keyword, string? status)
    {
        int? statusVal = MapStatusStringToInt(status);
        var projects = await _repo.GetAllAsync(portfolioId, programId, keyword, statusVal);

        return projects.Select(p => MapToDetailsDto(p));
    }

    public async Task<ProjectDetailsDto?> GetProjectByIdAsync(int id)
    {
        var project = await _repo.GetByIdAsync(id);
        return project != null ? MapToDetailsDto(project) : null;
    }

    public async Task<ProjectDetailsDto?> CreateProjectAsync(CreateProjectDto dto, string? userId)
    {
        // Validate Portfolio
        var portfolioExists = await _repo.PortfolioExistsAsync(dto.PortfolioId);
        if (!portfolioExists) return null;

        // Resolve Program ID
        int? resolvedProgramId = (dto.ProgramId.HasValue && dto.ProgramId.Value != 0) ? dto.ProgramId.Value : null;
        if (resolvedProgramId.HasValue)
        {
            var programExists = await _repo.ProgramExistsAsync(resolvedProgramId.Value);
            if (!programExists) return null;
        }

        var project = new Project.DAL.Entities.Project
        {
            Name = dto.Name,
            Description = dto.Description,
            Budget = dto.Budget,
            Status = MapStatusStringToInt(dto.Status) ?? 1,
            Priority = dto.Priority,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            PortfolioId = dto.PortfolioId,
            ProgramId = resolvedProgramId,
            ManagerId = userId ?? "default-user-id",
            ManagerName = dto.ManagerName ?? string.Empty,
            AttachedFiles = dto.AttachedFiles,
            CreatedDate = DateTime.UtcNow
        };

        await _repo.AddAsync(project);
        await _repo.SaveChangesAsync();

        var createdProject = await _repo.GetByIdAsync(project.Id);
        return MapToDetailsDto(createdProject ?? project);
    }

    public async Task<bool> UpdateProjectAsync(int id, UpdateProjectDto dto)
    {
        var project = await _repo.GetByIdAsync(id);
        if (project == null) return false;

        var portfolioExists = await _repo.PortfolioExistsAsync(dto.PortfolioId);
        if (!portfolioExists) return false;

        int? resolvedProgramId = (dto.ProgramId.HasValue && dto.ProgramId.Value != 0) ? dto.ProgramId.Value : null;
        if (resolvedProgramId.HasValue)
        {
            var programExists = await _repo.ProgramExistsAsync(resolvedProgramId.Value);
            if (!programExists) return false;
        }

        project.Name = dto.Name;
        project.Description = dto.Description;
        project.Budget = dto.Budget;
        project.Status = MapStatusStringToInt(dto.Status) ?? project.Status;
        project.Priority = dto.Priority;
        project.StartDate = dto.StartDate;
        project.EndDate = dto.EndDate;
        project.PortfolioId = dto.PortfolioId;
        project.ProgramId = resolvedProgramId;
        project.ManagerName = dto.ManagerName ?? project.ManagerName;
        project.AttachedFiles = dto.AttachedFiles;

        _repo.Update(project);
        return await _repo.SaveChangesAsync();
    }

    public async Task<bool> DeleteProjectAsync(int id)
    {
        var project = await _repo.GetByIdAsync(id);
        if (project == null) return false;

        _repo.Delete(project);
        return await _repo.SaveChangesAsync();
    }

    // Helper Status Mapping
    private int? MapStatusStringToInt(string? statusStr)
    {
        if (string.IsNullOrEmpty(statusStr)) return null;
        switch (statusStr.ToLower())
        {
            case "active":
            case "نشط":
                return 1;
            case "completed":
            case "مكتمل":
                return 2;
            case "pending":
            case "onhold":
            case "قيد الانتظار":
                return 3;
            case "rejected":
            case "refusing":
            case "مرفوض":
                return 4;
            default:
                return 1;
        }
    }

    // Helper map Entity to details DTO
    private ProjectDetailsDto MapToDetailsDto(Project.DAL.Entities.Project p)
    {
        return new ProjectDetailsDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Budget = p.Budget,
            Status = p.Status == 1 ? "Active" :
                     p.Status == 2 ? "Completed" :
                     p.Status == 3 ? "OnHold" : "Rejected",
            Priority = p.Priority,
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            ManagerName = !string.IsNullOrEmpty(p.ManagerName) ? p.ManagerName : (p.Manager != null ? p.Manager.UserName : "Not Assigned"),
            PortfolioName = p.Portfolio != null ? p.Portfolio.Name : "N/A",
            PortfolioId = p.PortfolioId,
            ProgramName = p.Program != null ? p.Program.Name : null,
            ProgramId = p.ProgramId,
            AttachedFiles = p.AttachedFiles,
            TasksCount = p.Tasks?.Count ?? 0,
            MembersCount = p.ProjectMembers?.Count ?? 0
        };
    }
}
