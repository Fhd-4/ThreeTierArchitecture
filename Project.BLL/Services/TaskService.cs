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
        int? statusVal = !string.IsNullOrEmpty(status) ? MapStatusStringToInt(status) : null;
        var tasks = await _repo.GetAllAsync(projectId, statusVal, keyword);
        return tasks.Select(MapToDetailsDto);
    }

    public async Task<TaskDetailsDto?> GetTaskByIdAsync(int id)
    {
        var task = await _repo.GetByIdAsync(id);
        return task != null ? MapToDetailsDto(task) : null;
    }

    public async Task<(TaskDetailsDto? Task, string? ErrorMessage)> CreateTaskAsync(CreateTaskDto dto, string? currentUserId)
    {
        var projectExists = await _repo.ProjectExistsAsync(dto.ProjectId);
        if (!projectExists)
            return (null, $"Project with ID {dto.ProjectId} does not exist. Please create a project first.");

        if (string.IsNullOrEmpty(currentUserId))
        {
            var defaultId = await _repo.GetDefaultUserIdAsync();
            currentUserId = defaultId ?? "default-user-id";
        }

        var task = new ProjectTask
        {
            Title = dto.Title,
            Description = dto.Description,
            Status = MapStatusStringToInt(dto.Status),
            Priority = dto.Priority,
            DueDate = dto.DueDate,
            ProjectId = dto.ProjectId,
            AssigneeName = dto.AssigneeName,
            AssigneeId = currentUserId,
            AttachedFiles = dto.AttachedFiles,
            CreatedDate = DateTime.UtcNow
        };

        await _repo.AddAsync(task);
        await _repo.SaveChangesAsync();

        var created = await _repo.GetByIdAsync(task.Id);
        return (MapToDetailsDto(created ?? task), null);
    }

    public async Task<(bool Success, string? ErrorMessage)> UpdateTaskAsync(int id, UpdateTaskDto dto)
    {
        var task = await _repo.GetByIdAsync(id);
        if (task == null)
            return (false, "Task not found.");

        var projectExists = await _repo.ProjectExistsAsync(dto.ProjectId);
        if (!projectExists)
            return (false, $"Project with ID {dto.ProjectId} does not exist.");

        task.Title = dto.Title;
        task.Description = dto.Description;
        task.Status = MapStatusStringToInt(dto.Status);
        task.Priority = dto.Priority;
        task.DueDate = dto.DueDate;
        task.ProjectId = dto.ProjectId;
        task.AssigneeName = dto.AssigneeName;
        task.AttachedFiles = dto.AttachedFiles;

        _repo.Update(task);
        var result = await _repo.SaveChangesAsync();
        return (result, null);
    }

    public async Task<bool> DeleteTaskAsync(int id)
    {
        var task = await _repo.GetByIdAsync(id);
        if (task == null) return false;

        _repo.Delete(task);
        return await _repo.SaveChangesAsync();
    }

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