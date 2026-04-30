using Microsoft.EntityFrameworkCore;
using TaskApi.Models;

namespace TaskApi.Repositories;

/// <summary>
/// Repository implementation for task data access operations
/// </summary>
public class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<TaskRepository> _logger;

    public TaskRepository(AppDbContext context, ILogger<TaskRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<TaskItem>> GetAllAsync()
    {
        _logger.LogInformation("Retrieving all tasks from database");
        return await _context.Tasks.OrderBy(t => t.CreatedAt).ToListAsync();
    }

    public async Task<IEnumerable<TaskItem>> GetByStatusAsync(bool isCompleted)
    {
        _logger.LogInformation("Retrieving tasks with completion status: {IsCompleted}", isCompleted);
        return await _context.Tasks
            .Where(t => t.IsCompleted == isCompleted)
            .OrderBy(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<TaskItem?> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Retrieving task with ID: {TaskId}", id);
        return await _context.Tasks.FindAsync(id);
    }

    public async Task<TaskItem> AddAsync(TaskItem task)
    {
        _logger.LogInformation("Adding new task with title: {Title}", task.Title);
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Task created successfully with ID: {TaskId}", task.Id);
        return task;
    }

    public async Task<TaskItem> UpdateAsync(TaskItem task)
    {
        _logger.LogInformation("Updating task with ID: {TaskId}", task.Id);
        _context.Tasks.Update(task);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Task updated successfully: {TaskId}", task.Id);
        return task;
    }

    public async Task DeleteAsync(Guid id)
    {
        _logger.LogInformation("Deleting task with ID: {TaskId}", id);
        var task = await _context.Tasks.FindAsync(id);
        if (task != null)
        {
            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Task deleted successfully: {TaskId}", id);
        }
    }
}
