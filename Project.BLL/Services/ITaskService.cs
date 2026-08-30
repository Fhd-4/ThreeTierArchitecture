using Project.BLL.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.BLL.Services;

public interface ITaskService
{
    Task<IEnumerable<TaskDetailsDto>> GetTasksAsync(int? projectId, string? status, string? keyword);
    Task<TaskDetailsDto?> GetTaskByIdAsync(int id);
    Task<(TaskDetailsDto? Task, string? ErrorMessage)> CreateTaskAsync(CreateTaskDto dto, string? currentUserId);
    Task<(bool Success, string? ErrorMessage)> UpdateTaskAsync(int id, UpdateTaskDto dto);
    Task<bool> DeleteTaskAsync(int id);
}