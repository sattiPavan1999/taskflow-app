using TaskApi.DTOs;

namespace TaskApi.Services;

public interface ITaskService
{
    Task<IEnumerable<TaskResponseDto>> GetTasksAsync(int userId, string? status = "all");
    Task<TaskResponseDto> GetTaskByIdAsync(int userId, int id);
    Task<TaskResponseDto> CreateTaskAsync(int userId, CreateTaskDto createDto);
    Task<TaskResponseDto> UpdateTaskAsync(int userId, int id, UpdateTaskDto updateDto);
    Task DeleteTaskAsync(int userId, int id);
}
