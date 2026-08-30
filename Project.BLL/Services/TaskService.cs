using Project.BLL.DTOs;
using Project.DAL.Entities;
using Project.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.BLL.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _repo;

    public TaskService(ITaskRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<TaskDetailsDto>> GetTasksAsync(int? projectId, string? status, string? keyword)
    {
        int? statusVal = null;
        if (!string.IsNullOrEmpty(status))
        {
            statusVal = MapStatusStringToInt(status);
        }

        var tasks = await _repo.GetAllTasksAsync(projectId, statusVal, keyword);
        return tasks.Select(t => MapToDetailsDto(t));
    }

    public async Task<TaskDetailsDto?> GetTaskByIdAsync(int id)
    {
        var task = await _repo.GetByIdAsync(id);
        return task != null ? MapToDetailsDto(task) : null;
    }

    public async Task<TaskDetailsDto?> CreateTaskAsync(CreateTaskDto dto, string? userId)
    {
        // Check if project exists
        var projectExists = await _repo.ProjectExistsAsync(dto.ProjectId);
        if (!projectExists) return null;

        var task = new ProjectTask
        {
            Title = dto.Title,
            Description = dto.Description,
            Status = MapStatusStringToInt(dto.Status),
            Priority = dto.Priority,
            DueDate = dto.DueDate,
            ProjectId = dto.ProjectId,
            AssigneeName = dto.AssigneeName ?? string.Empty,
            AssigneeId = userId ?? "default-user-id", // Assign current user fallback
            AttachedFiles = dto.AttachedFiles,
            CreatedDate = DateTime.UtcNow
        };

        await _repo.AddAsync(task);
        await _repo.SaveChangesAsync();

        var createdTask = await _repo.GetByIdAsync(task.Id);
        return MapToDetailsDto(createdTask ?? task);
    }

    public async Task<bool> UpdateTaskAsync(int id, UpdateTaskDto dto)
    {
        var task = await _repo.GetByIdAsync(id);
        if (task == null) return false;

        var projectExists = await _repo.ProjectExistsAsync(dto.ProjectId);
        if (!projectExists) return false;

        task.Title = dto.Title;
        task.Description = dto.Description;
        task.Status = MapStatusStringToInt(dto.Status);
        task.Priority = dto.Priority;
        task.DueDate = dto.DueDate;
        task.ProjectId = dto.ProjectId;
        task.AssigneeName = dto.AssigneeName ?? string.Empty;
        task.AttachedFiles = dto.AttachedFiles;

        _repo.Update(task);
        return await _repo.SaveChangesAsync();
    }

    public async Task<bool> DeleteTaskAsync(int id)
    {
        var task = await _repo.GetByIdAsync(id);
        if (task == null) return false;

        _repo.Delete(task);
        return await _repo.SaveChangesAsync();
    }

    // Helper: Map status string to int
    private int MapStatusStringToInt(string? statusStr)
    {
        if (string.IsNullOrEmpty(statusStr)) return 1;
        switch (statusStr.ToLower().Replace(" ", "").Replace("-", ""))
        {
            case "todo":
                return 1;
            case "inprogress":
                return 2;
            case "inreview":
                return 3;
            case "done":
                return 4;
            default:
                return 1;
        }
    }

    // Helper: Map status int to string
    private string MapStatusIntToString(int status)
    {
        switch (status)
        {
            case 1: return "To Do";
            case 2: return "In Progress";
            case 3: return "In Review";
            case 4: return "Done";
            default: return "To Do";
        }
    }

    // Helper: Map Entity to Details DTO
    private TaskDetailsDto MapToDetailsDto(ProjectTask t)
    {
        return new TaskDetailsDto
        {
            Id = t.Id,
            Title = t.Title,
            Description = t.Description,
            Status = MapStatusIntToString(t.Status),
            Priority = t.Priority,
            DueDate = t.DueDate,
            CreatedDate = t.CreatedDate,
            ProjectId = t.ProjectId,
            ProjectName = t.Project != null ? t.Project.Name : "N/A",
            AssigneeName = !string.IsNullOrEmpty(t.AssigneeName) ? t.AssigneeName : (t.Assignee != null ? t.Assignee.UserName : "Not Assigned"),
            AttachedFiles = t.AttachedFiles
        };
    }
}
