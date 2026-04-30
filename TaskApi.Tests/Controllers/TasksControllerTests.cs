using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TaskApi.Controllers;
using TaskApi.DTOs;
using TaskApi.Services;

namespace TaskApi.Tests.Controllers;

public class TasksControllerTests
{
    private readonly Mock<ITaskService> _service = new();
    private readonly Mock<ILogger<TasksController>> _logger = new();
    private TasksController CreateController() => new(_service.Object, _logger.Object);

    private static TaskResponseDto MakeResponse(string title = "Test", bool completed = false) =>
        new() { Id = Guid.NewGuid(), Title = title, IsCompleted = completed, Priority = "medium", CreatedAt = DateTime.UtcNow };

    // ── GET /api/tasks ─────────────────────────────────────────────

    [Fact]
    public async Task GetTasks_Returns200_WithTaskList()
    {
        var tasks = new List<TaskResponseDto> { MakeResponse("A"), MakeResponse("B") };
        _service.Setup(s => s.GetTasksAsync("all")).ReturnsAsync(tasks);

        var result = await CreateController().GetTasks("all");

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var returned = Assert.IsAssignableFrom<IEnumerable<TaskResponseDto>>(ok.Value);
        Assert.Equal(2, returned.Count());
    }

    [Fact]
    public async Task GetTasks_PassesStatusFilterToService()
    {
        _service.Setup(s => s.GetTasksAsync("active")).ReturnsAsync(new List<TaskResponseDto>());

        await CreateController().GetTasks("active");

        _service.Verify(s => s.GetTasksAsync("active"), Times.Once);
    }

    // ── GET /api/tasks/{id} ────────────────────────────────────────

    [Fact]
    public async Task GetTask_Returns200_WithTask()
    {
        var task = MakeResponse("My Task");
        _service.Setup(s => s.GetTaskByIdAsync(task.Id)).ReturnsAsync(task);

        var result = await CreateController().GetTask(task.Id);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var returned = Assert.IsType<TaskResponseDto>(ok.Value);
        Assert.Equal(task.Id, returned.Id);
    }

    // ── POST /api/tasks ────────────────────────────────────────────

    [Fact]
    public async Task CreateTask_Returns201_WithCreatedTask()
    {
        var dto = new CreateTaskDto { Title = "New Task", Priority = "high", Category = "Work" };
        var created = MakeResponse("New Task");
        _service.Setup(s => s.CreateTaskAsync(dto)).ReturnsAsync(created);

        var result = await CreateController().CreateTask(dto);

        var createdAt = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(201, createdAt.StatusCode);
        var returned = Assert.IsType<TaskResponseDto>(createdAt.Value);
        Assert.Equal(created.Id, returned.Id);
    }

    [Fact]
    public async Task CreateTask_ReturnsLocationHeader_PointingToGetTask()
    {
        var dto = new CreateTaskDto { Title = "Task", Priority = "medium" };
        var created = MakeResponse();
        _service.Setup(s => s.CreateTaskAsync(dto)).ReturnsAsync(created);

        var result = await CreateController().CreateTask(dto);

        var createdAt = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(TasksController.GetTask), createdAt.ActionName);
    }

    // ── PUT /api/tasks/{id} ────────────────────────────────────────

    [Fact]
    public async Task UpdateTask_Returns200_WithUpdatedTask()
    {
        var id = Guid.NewGuid();
        var dto = new UpdateTaskDto { Title = "Updated", IsCompleted = true, Priority = "low" };
        var updated = MakeResponse("Updated", completed: true);
        _service.Setup(s => s.UpdateTaskAsync(id, dto)).ReturnsAsync(updated);

        var result = await CreateController().UpdateTask(id, dto);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var returned = Assert.IsType<TaskResponseDto>(ok.Value);
        Assert.True(returned.IsCompleted);
    }

    // ── DELETE /api/tasks/{id} ─────────────────────────────────────

    [Fact]
    public async Task DeleteTask_Returns204_NoContent()
    {
        var id = Guid.NewGuid();
        _service.Setup(s => s.DeleteTaskAsync(id)).Returns(Task.CompletedTask);

        var result = await CreateController().DeleteTask(id);

        Assert.IsType<NoContentResult>(result);
        _service.Verify(s => s.DeleteTaskAsync(id), Times.Once);
    }
}
