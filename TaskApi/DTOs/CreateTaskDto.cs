using System.ComponentModel.DataAnnotations;

namespace TaskApi.DTOs;

/// <summary>
/// Data transfer object for creating a new task
/// </summary>
public class CreateTaskDto
{
    /// <summary>
    /// Title of the task
    /// </summary>
    [Required(ErrorMessage = "Title is required")]
    [StringLength(500, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 500 characters")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Optional description of the task
    /// </summary>
    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    public string? Description { get; set; }

    public string Priority { get; set; } = "medium";
    public string? Category { get; set; }
    public DateTime? DueDate { get; set; }
}
