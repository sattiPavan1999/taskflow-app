using Microsoft.Extensions.Logging;
using Moq;
using TaskApi.DTOs;
using TaskApi.Exceptions;
using TaskApi.Models;
using TaskApi.Repositories;
using TaskApi.Services;

namespace TaskApi.Tests.Services;

public class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _repo = new();
    private readonly Mock<ILogger<TaskService>> _logger = new();
    private TaskService CreateService() => new(_repo.Object, _logger.Object);

    private static TaskItem MakeTask(string title = "Test", bool completed = false, string priority = "medium", DateTime? dueDate = null) =>
        new() { Id = Guid.NewGuid(), UserId = 1, Title = title, IsCompleted = completed, Priority = priority, DueDate = dueDate, CreatedAt = DateTime.UtcNow };

    // ── GetTasksAsync ──────────────────────────────────────────────

    [Fact]
    public async Task GetTasksAsync_ReturnsAllTasks()
    {
        var tasks = new List<TaskItem> { MakeTask("A"), MakeTask("B"), MakeTask("C") };
        _repo.Setup(r => r.GetAllAsync(It.IsAny<int>())).ReturnsAsync(tasks);

        var result = await CreateService().GetTasksAsync(1, "all");

        Assert.Equal(3, result.Count());
    }

    [Fact]
    public async Task GetTasksAsync_ActiveFilter_ReturnsOnlyActiveTasks()
    {
        var tasks = new List<TaskItem> { MakeTask("Active"), MakeTask("Done", completed: true) };
        _repo.Setup(r => r.GetByStatusAsync(false, It.IsAny<int>())).ReturnsAsync(tasks.Where(t => !t.IsCompleted));

        var result = await CreateService().GetTasksAsync(1, "active");

        Assert.Single(result);
        Assert.Equal("Active", result.First().Title);
    }

    [Fact]
    public async Task GetTasksAsync_CompletedFilter_ReturnsOnlyCompletedTasks()
    {
        var tasks = new List<TaskItem> { MakeTask("Done", completed: true) };
        _repo.Setup(r => r.GetByStatusAsync(true, It.IsAny<int>())).ReturnsAsync(tasks);

        var result = await CreateService().GetTasksAsync(1, "completed");

        Assert.Single(result);
        Assert.Equal("Done", result.First().Title);
    }

    [Fact]
    public async Task GetTasksAsync_OverdueFilter_ReturnsOnlyOverdueTasks()
    {
        var overdue = MakeTask("Late", dueDate: DateTime.UtcNow.AddDays(-1));
        var future = MakeTask("Future", dueDate: DateTime.UtcNow.AddDays(5));
        _repo.Setup(r => r.GetAllAsync(It.IsAny<int>())).ReturnsAsync(new List<TaskItem> { overdue, future });

        var result = await CreateService().GetTasksAsync(1, "overdue");

        Assert.Single(result);
        Assert.Equal("Late", result.First().Title);
    }

    [Fact]
    public async Task GetTasksAsync_InvalidStatus_ThrowsValidationException()
    {
        await Assert.ThrowsAsync<ValidationException>(() => CreateService().GetTasksAsync(1, "unknown"));
    }

    // ── GetTaskByIdAsync ───────────────────────────────────────────

    [Fact]
    public async Task GetTaskByIdAsync_ReturnsTask_WhenFound()
    {
        var task = MakeTask("My Task");
        _repo.Setup(r => r.GetByIdAsync(task.Id, It.IsAny<int>())).ReturnsAsync(task);

        var result = await CreateService().GetTaskByIdAsync(1, task.Id);

        Assert.Equal(task.Id, result.Id);
        Assert.Equal("My Task", result.Title);
    }

    [Fact]
    public async Task GetTaskByIdAsync_ThrowsNotFoundException_WhenNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<int>())).ReturnsAsync((TaskItem?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => CreateService().GetTaskByIdAsync(1, Guid.NewGuid()));
    }

    // ── CreateTaskAsync ────────────────────────────────────────────

    [Fact]
    public async Task CreateTaskAsync_CreatesTaskWithCorrectFields()
    {
        var dto = new CreateTaskDto { Title = "New Task", Priority = "high", Category = "Work", DueDate = DateTime.UtcNow.AddDays(3) };
        _repo.Setup(r => r.AddAsync(It.IsAny<TaskItem>())).ReturnsAsync((TaskItem t) => t);

        var result = await CreateService().CreateTaskAsync(1, dto);

        Assert.Equal("New Task", result.Title);
        Assert.Equal("high", result.Priority);
        Assert.Equal("Work", result.Category);
        Assert.False(result.IsCompleted);
        Assert.NotEqual(Guid.Empty, result.Id);
        _repo.Verify(r => r.AddAsync(It.IsAny<TaskItem>()), Times.Once);
    }

    // ── UpdateTaskAsync ────────────────────────────────────────────

    [Fact]
    public async Task UpdateTaskAsync_UpdatesAndReturnsTask()
    {
        var existing = MakeTask("Old Title");
        var dto = new UpdateTaskDto { Title = "New Title", IsCompleted = true, Priority = "low" };
        _repo.Setup(r => r.GetByIdAsync(existing.Id, It.IsAny<int>())).ReturnsAsync(existing);
        _repo.Setup(r => r.UpdateAsync(It.IsAny<TaskItem>())).ReturnsAsync((TaskItem t) => t);

        var result = await CreateService().UpdateTaskAsync(1, existing.Id, dto);

        Assert.Equal("New Title", result.Title);
        Assert.True(result.IsCompleted);
        Assert.Equal("low", result.Priority);
        _repo.Verify(r => r.UpdateAsync(It.IsAny<TaskItem>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTaskAsync_ThrowsNotFoundException_WhenNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<int>())).ReturnsAsync((TaskItem?)null);
        var dto = new UpdateTaskDto { Title = "X", IsCompleted = false, Priority = "medium" };

        await Assert.ThrowsAsync<NotFoundException>(() => CreateService().UpdateTaskAsync(1, Guid.NewGuid(), dto));
    }

    // ── DeleteTaskAsync ────────────────────────────────────────────

    [Fact]
    public async Task DeleteTaskAsync_DeletesTask_WhenFound()
    {
        var task = MakeTask();
        _repo.Setup(r => r.GetByIdAsync(task.Id, It.IsAny<int>())).ReturnsAsync(task);
        _repo.Setup(r => r.DeleteAsync(task.Id, It.IsAny<int>())).Returns(Task.CompletedTask);

        await CreateService().DeleteTaskAsync(1, task.Id);

        _repo.Verify(r => r.DeleteAsync(task.Id, It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task DeleteTaskAsync_ThrowsNotFoundException_WhenNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<int>())).ReturnsAsync((TaskItem?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => CreateService().DeleteTaskAsync(1, Guid.NewGuid()));
    }
}
