using TaskApi.DTOs;
using TaskApi.Exceptions;
using TaskApi.Models;
using TaskApi.Repositories;

namespace TaskApi.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _repository;
    private readonly ILogger<TaskService> _logger;

    public TaskService(ITaskRepository repository, ILogger<TaskService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<TaskResponseDto>> GetTasksAsync(int userId, string? status = "all")
    {
        _logger.LogInformation("Fetching tasks with status filter: {Status}", status ?? "all");

        var normalizedStatus = status?.ToLowerInvariant() ?? "all";

        if (normalizedStatus != "all" && normalizedStatus != "active" && normalizedStatus != "completed" && normalizedStatus != "overdue")
        {
            throw new ValidationException($"Invalid status filter: {status}. Valid values are: all, active, completed, overdue");
        }

        IEnumerable<TaskItem> tasks;

        if (normalizedStatus == "all")
        {
            tasks = await _repository.GetAllAsync(userId);
        }
        else if (normalizedStatus == "overdue")
        {
            var now = DateTime.UtcNow;
            tasks = (await _repository.GetAllAsync(userId))
                .Where(t => !t.IsCompleted && t.DueDate.HasValue && t.DueDate.Value < now);
        }
        else
        {
            var isCompleted = normalizedStatus == "completed";
            tasks = await _repository.GetByStatusAsync(isCompleted, userId);
        }

        return tasks.Select(MapToResponseDto);
    }

    public async Task<TaskResponseDto> GetTaskByIdAsync(int userId, int id)
    {
        _logger.LogInformation("Fetching task by ID: {TaskId}", id);

        var task = await _repository.GetByIdAsync(id, userId);
        if (task == null)
        {
            throw new NotFoundException($"Task with ID {id} not found");
        }

        return MapToResponseDto(task);
    }

    public async Task<TaskResponseDto> CreateTaskAsync(int userId, CreateTaskDto createDto)
    {
        _logger.LogInformation("Creating new task with title: {Title}", createDto.Title);

        var task = new TaskItem
        {
            UserId = userId,
            Title = createDto.Title,
            Description = createDto.Description,
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow,
            Priority = createDto.Priority,
            Category = createDto.Category,
            DueDate = createDto.DueDate
        };

        var createdTask = await _repository.AddAsync(task);
        return MapToResponseDto(createdTask);
    }

    public async Task<TaskResponseDto> UpdateTaskAsync(int userId, int id, UpdateTaskDto updateDto)
    {
        _logger.LogInformation("Updating task with ID: {TaskId}", id);

        var existingTask = await _repository.GetByIdAsync(id, userId);
        if (existingTask == null)
        {
            throw new NotFoundException($"Task with ID {id} not found");
        }

        existingTask.Title = updateDto.Title;
        existingTask.Description = updateDto.Description;
        existingTask.IsCompleted = updateDto.IsCompleted;
        existingTask.Priority = updateDto.Priority;
        existingTask.Category = updateDto.Category;
        existingTask.DueDate = updateDto.DueDate;

        var updatedTask = await _repository.UpdateAsync(existingTask);
        return MapToResponseDto(updatedTask);
    }

    public async Task DeleteTaskAsync(int userId, int id)
    {
        _logger.LogInformation("Deleting task with ID: {TaskId}", id);

        var task = await _repository.GetByIdAsync(id, userId);
        if (task == null)
        {
            throw new NotFoundException($"Task with ID {id} not found");
        }

        await _repository.DeleteAsync(id, userId);
    }

    private static TaskResponseDto MapToResponseDto(TaskItem task)
    {
        return new TaskResponseDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            IsCompleted = task.IsCompleted,
            CreatedAt = task.CreatedAt,
            Priority = task.Priority,
            Category = task.Category,
            DueDate = task.DueDate
        };
    }
}
