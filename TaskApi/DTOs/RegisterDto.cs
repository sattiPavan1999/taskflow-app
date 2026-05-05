using System.ComponentModel.DataAnnotations;

namespace TaskApi.DTOs;

public class RegisterDto
{
    [Required]
    [RegularExpression(@"^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}$",
        ErrorMessage = "Enter a valid email address (e.g. abc@email.com)")]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;
}
