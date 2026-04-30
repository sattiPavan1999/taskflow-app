using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TaskApi.Models;
using TaskApi.Repositories;

namespace TaskApi.Tests.Repositories;

public class TaskRepositoryTests : IDisposable
{
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

    private static TaskItem MakeTask(string title = "Test", bool completed = false) =>
        new() { Id = Guid.NewGuid(), Title = title, IsCompleted = completed, Priority = "medium", CreatedAt = DateTime.UtcNow };

    // ── GetAllAsync ────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ReturnsAllTasks()
    {
        await _repo.AddAsync(MakeTask("A"));
        await _repo.AddAsync(MakeTask("B"));

        var result = await _repo.GetAllAsync();

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmpty_WhenNoTasks()
    {
        var result = await _repo.GetAllAsync();

        Assert.Empty(result);
    }

    // ── GetByStatusAsync ───────────────────────────────────────────

    [Fact]
    public async Task GetByStatusAsync_ReturnsOnlyMatchingTasks()
    {
        await _repo.AddAsync(MakeTask("Active", completed: false));
        await _repo.AddAsync(MakeTask("Done", completed: true));

        var active = await _repo.GetByStatusAsync(false);
        var completed = await _repo.GetByStatusAsync(true);

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

        var result = await _repo.GetByIdAsync(task.Id);

        Assert.NotNull(result);
        Assert.Equal(task.Id, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        var result = await _repo.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    // ── AddAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task AddAsync_PersistsTask()
    {
        var task = MakeTask("New Task");

        await _repo.AddAsync(task);

        var saved = await _repo.GetByIdAsync(task.Id);
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

        var saved = await _repo.GetByIdAsync(task.Id);
        Assert.Equal("Updated", saved!.Title);
        Assert.True(saved.IsCompleted);
    }

    // ── DeleteAsync ────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_RemovesTask()
    {
        var task = MakeTask("To Delete");
        await _repo.AddAsync(task);

        await _repo.DeleteAsync(task.Id);

        var result = await _repo.GetByIdAsync(task.Id);
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_DoesNotThrow_WhenTaskDoesNotExist()
    {
        var exception = await Record.ExceptionAsync(() => _repo.DeleteAsync(Guid.NewGuid()));

        Assert.Null(exception);
    }
}
