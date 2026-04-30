using TaskApi.DTOs;

namespace TaskApi.Services;

/// <summary>
/// Service interface for task business logic operations
/// </summary>
public interface ITaskService
{
    /// <summary>
    /// Retrieves tasks based on optional status filter
    /// </summary>
    /// <param name="status">Filter status: "all", "active", or "completed"</param>
    Task<IEnumerable<TaskResponseDto>> GetTasksAsync(string? status = "all");

    /// <summary>
    /// Retrieves a task by its unique identifier
    /// </summary>
    /// <param name="id">Task identifier</param>
    Task<TaskResponseDto> GetTaskByIdAsync(Guid id);

    /// <summary>
    /// Creates a new task
    /// </summary>
    /// <param name="createDto">Task creation data</param>
    Task<TaskResponseDto> CreateTaskAsync(CreateTaskDto createDto);

    /// <summary>
    /// Updates an existing task
    /// </summary>
    /// <param name="id">Task identifier</param>
    /// <param name="updateDto">Task update data</param>
    Task<TaskResponseDto> UpdateTaskAsync(Guid id, UpdateTaskDto updateDto);

    /// <summary>
    /// Deletes a task
    /// </summary>
    /// <param name="id">Task identifier</param>
    Task DeleteTaskAsync(Guid id);
}
