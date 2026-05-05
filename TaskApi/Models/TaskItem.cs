namespace TaskApi.Models;

/// <summary>
/// Domain entity representing a task item
/// </summary>
public class TaskItem
{
    /// <summary>
    /// Unique identifier for the task
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Title of the task (required)
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Optional detailed description of the task
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Indicates whether the task is completed
    /// </summary>
    public bool IsCompleted { get; set; }

    /// <summary>
    /// Timestamp when the task was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    public string Priority { get; set; } = "medium";
    public string? Category { get; set; }
    public DateTime? DueDate { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
}
