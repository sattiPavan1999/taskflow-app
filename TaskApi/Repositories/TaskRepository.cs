using Microsoft.EntityFrameworkCore;
using TaskApi.Models;

namespace TaskApi.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<TaskRepository> _logger;

    public TaskRepository(AppDbContext context, ILogger<TaskRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<TaskItem>> GetAllAsync(int userId)
    {
        _logger.LogInformation("Retrieving all tasks for user {UserId}", userId);
        return await _context.Tasks
            .Where(t => t.UserId == userId)
            .OrderBy(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<TaskItem>> GetByStatusAsync(bool isCompleted, int userId)
    {
        _logger.LogInformation("Retrieving tasks with status {IsCompleted} for user {UserId}", isCompleted, userId);
        return await _context.Tasks
            .Where(t => t.IsCompleted == isCompleted && t.UserId == userId)
            .OrderBy(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<TaskItem?> GetByIdAsync(int id, int userId)
    {
        _logger.LogInformation("Retrieving task {TaskId} for user {UserId}", id, userId);
        return await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
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

    public async Task DeleteAsync(int id, int userId)
    {
        _logger.LogInformation("Deleting task {TaskId} for user {UserId}", id, userId);
        var task = await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
        if (task != null)
        {
            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Task deleted successfully: {TaskId}", id);
        }
    }
}
