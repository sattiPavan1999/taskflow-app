using System.Security.Claims;
using Microsoft.AspNetCore.Http;
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

    private TasksController CreateController()
    {
        var controller = new TasksController(_service.Object, _logger.Object);
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "1") };
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"))
            }
        };
        return controller;
    }

    private static int _nextId = 1;
    private static TaskResponseDto MakeResponse(string title = "Test", bool completed = false) =>
        new() { Id = _nextId++, Title = title, IsCompleted = completed, Priority = "medium", CreatedAt = DateTime.UtcNow };

    // ── GET /api/tasks ─────────────────────────────────────────────

    [Fact]
    public async Task GetTasks_Returns200_WithTaskList()
    {
        var tasks = new List<TaskResponseDto> { MakeResponse("A"), MakeResponse("B") };
        _service.Setup(s => s.GetTasksAsync(It.IsAny<int>(), "all")).ReturnsAsync(tasks);

        var result = await CreateController().GetTasks("all");

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var returned = Assert.IsAssignableFrom<IEnumerable<TaskResponseDto>>(ok.Value);
        Assert.Equal(2, returned.Count());
    }

    [Fact]
    public async Task GetTasks_PassesStatusFilterToService()
    {
        _service.Setup(s => s.GetTasksAsync(It.IsAny<int>(), "active")).ReturnsAsync(new List<TaskResponseDto>());

        await CreateController().GetTasks("active");

        _service.Verify(s => s.GetTasksAsync(It.IsAny<int>(), "active"), Times.Once);
    }

    // ── GET /api/tasks/{id} ────────────────────────────────────────

    [Fact]
    public async Task GetTask_Returns200_WithTask()
    {
        var task = MakeResponse("My Task");
        _service.Setup(s => s.GetTaskByIdAsync(It.IsAny<int>(), task.Id)).ReturnsAsync(task);

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
        _service.Setup(s => s.CreateTaskAsync(It.IsAny<int>(), dto)).ReturnsAsync(created);

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
        _service.Setup(s => s.CreateTaskAsync(It.IsAny<int>(), dto)).ReturnsAsync(created);

        var result = await CreateController().CreateTask(dto);

        var createdAt = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(TasksController.GetTask), createdAt.ActionName);
    }

    // ── PUT /api/tasks/{id} ────────────────────────────────────────

    [Fact]
    public async Task UpdateTask_Returns200_WithUpdatedTask()
    {
        var id = 1;
        var dto = new UpdateTaskDto { Title = "Updated", IsCompleted = true, Priority = "low" };
        var updated = MakeResponse("Updated", completed: true);
        _service.Setup(s => s.UpdateTaskAsync(It.IsAny<int>(), id, dto)).ReturnsAsync(updated);

        var result = await CreateController().UpdateTask(id, dto);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var returned = Assert.IsType<TaskResponseDto>(ok.Value);
        Assert.True(returned.IsCompleted);
    }

    // ── DELETE /api/tasks/{id} ─────────────────────────────────────

    [Fact]
    public async Task DeleteTask_Returns204_NoContent()
    {
        var id = 1;
        _service.Setup(s => s.DeleteTaskAsync(It.IsAny<int>(), id)).Returns(Task.CompletedTask);

        var result = await CreateController().DeleteTask(id);

        Assert.IsType<NoContentResult>(result);
        _service.Verify(s => s.DeleteTaskAsync(It.IsAny<int>(), id), Times.Once);
    }
}
