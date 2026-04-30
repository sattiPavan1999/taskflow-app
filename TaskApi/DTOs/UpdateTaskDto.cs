using System.ComponentModel.DataAnnotations;

namespace TaskApi.DTOs;

/// <summary>
/// Data transfer object for updating an existing task
/// </summary>
public class UpdateTaskDto
{
    /// <summary>
    /// Updated title of the task
    /// </summary>
    [Required(ErrorMessage = "Title is required")]
    [StringLength(500, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 500 characters")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Updated description of the task
    /// </summary>
    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    public string? Description { get; set; }

    /// <summary>
    /// Completion status of the task
    /// </summary>
    [Required(ErrorMessage = "IsCompleted is required")]
    public bool IsCompleted { get; set; }

    public string Priority { get; set; } = "medium";
    public string? Category { get; set; }
    public DateTime? DueDate { get; set; }
}
