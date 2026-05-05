using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TaskApi.Models;
using TaskApi.Repositories;

namespace TaskApi.Tests.Repositories;

public class TaskRepositoryTests : IDisposable
{
    private const int UserId = 1;
    private const int OtherUserId = 2;

    private readonly AppDbContext _context;
    private readonly TaskRepository _repo;

    public TaskRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _repo = new TaskRepository(_context, new Mock<ILogger<TaskRepository>>().Object);
    }

    public void Dispose() => _context.Dispose();

    private static TaskItem MakeTask(string title = "Test", bool completed = false, int userId = UserId) =>
        new() { Id = Guid.NewGuid(), UserId = userId, Title = title, IsCompleted = completed, Priority = "medium", CreatedAt = DateTime.UtcNow };

    // ── GetAllAsync ────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ReturnsAllTasksForUser()
    {
        await _repo.AddAsync(MakeTask("A"));
        await _repo.AddAsync(MakeTask("B"));

        var result = await _repo.GetAllAsync(UserId);

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmpty_WhenNoTasks()
    {
        var result = await _repo.GetAllAsync(UserId);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_DoesNotReturnOtherUsersTasks()
    {
        await _repo.AddAsync(MakeTask("Mine"));
        await _repo.AddAsync(MakeTask("Theirs", userId: OtherUserId));

        var result = await _repo.GetAllAsync(UserId);

        Assert.Single(result);
        Assert.Equal("Mine", result.First().Title);
    }

    // ── GetByStatusAsync ───────────────────────────────────────────

    [Fact]
    public async Task GetByStatusAsync_ReturnsOnlyMatchingTasks()
    {
        await _repo.AddAsync(MakeTask("Active", completed: false));
        await _repo.AddAsync(MakeTask("Done", completed: true));

        var active = await _repo.GetByStatusAsync(false, UserId);
        var completed = await _repo.GetByStatusAsync(true, UserId);

        Assert.Single(active);
        Assert.Equal("Active", active.First().Title);
        Assert.Single(completed);
        Assert.Equal("Done", completed.First().Title);
    }

    // ── GetByIdAsync ───────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ReturnsTask_WhenFound()
    {
        var task = MakeTask("Find Me");
        await _repo.AddAsync(task);

        var result = await _repo.GetByIdAsync(task.Id, UserId);

        Assert.NotNull(result);
        Assert.Equal(task.Id, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        var result = await _repo.GetByIdAsync(Guid.NewGuid(), UserId);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenTaskBelongsToOtherUser()
    {
        var task = MakeTask("Theirs", userId: OtherUserId);
        await _repo.AddAsync(task);

        var result = await _repo.GetByIdAsync(task.Id, UserId);

        Assert.Null(result);
    }

    // ── AddAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task AddAsync_PersistsTask()
    {
        var task = MakeTask("New Task");

        await _repo.AddAsync(task);

        var saved = await _repo.GetByIdAsync(task.Id, UserId);
        Assert.NotNull(saved);
        Assert.Equal("New Task", saved.Title);
    }

    // ── UpdateAsync ────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_SavesChanges()
    {
        var task = MakeTask("Original");
        await _repo.AddAsync(task);

        task.Title = "Updated";
        task.IsCompleted = true;
        await _repo.UpdateAsync(task);

        var saved = await _repo.GetByIdAsync(task.Id, UserId);
        Assert.Equal("Updated", saved!.Title);
        Assert.True(saved.IsCompleted);
    }

    // ── DeleteAsync ────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_RemovesTask()
    {
        var task = MakeTask("To Delete");
        await _repo.AddAsync(task);

        await _repo.DeleteAsync(task.Id, UserId);

        var result = await _repo.GetByIdAsync(task.Id, UserId);
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_DoesNotThrow_WhenTaskDoesNotExist()
    {
        var exception = await Record.ExceptionAsync(() => _repo.DeleteAsync(Guid.NewGuid(), UserId));

        Assert.Null(exception);
    }
}
