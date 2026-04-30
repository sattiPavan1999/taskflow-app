using TaskApi.Models;

namespace TaskApi.Repositories;

/// <summary>
/// Repository interface for task data access operations
/// </summary>
public interface ITaskRepository
{
    /// <summary>
    /// Retrieves all tasks from the database
    /// </summary>
    Task<IEnumerable<TaskItem>> GetAllAsync();

    /// <summary>
    /// Retrieves tasks filtered by completion status
    /// </summary>
    /// <param name="isCompleted">Filter by completion status</param>
    Task<IEnumerable<TaskItem>> GetByStatusAsync(bool isCompleted);

    /// <summary>
    /// Retrieves a task by its unique identifier
    /// </summary>
    /// <param name="id">Task identifier</param>
    Task<TaskItem?> GetByIdAsync(Guid id);

    /// <summary>
    /// Adds a new task to the database
    /// </summary>
    /// <param name="task">Task to add</param>
    Task<TaskItem> AddAsync(TaskItem task);

    /// <summary>
    /// Updates an existing task in the database
    /// </summary>
    /// <param name="task">Task to update</param>
    Task<TaskItem> UpdateAsync(TaskItem task);

    /// <summary>
    /// Deletes a task from the database
    /// </summary>
    /// <param name="id">Task identifier</param>
    Task DeleteAsync(Guid id);
}
