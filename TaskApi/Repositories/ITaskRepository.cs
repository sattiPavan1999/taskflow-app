using TaskApi.Models;

namespace TaskApi.Repositories;

public interface ITaskRepository
{
    Task<IEnumerable<TaskItem>> GetAllAsync(int userId);
    Task<IEnumerable<TaskItem>> GetByStatusAsync(bool isCompleted, int userId);
    Task<TaskItem?> GetByIdAsync(int id, int userId);
    Task<TaskItem> AddAsync(TaskItem task);
    Task<TaskItem> UpdateAsync(TaskItem task);
    Task DeleteAsync(int id, int userId);
}
