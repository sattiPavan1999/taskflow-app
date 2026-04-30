namespace TaskApi.DTOs;

/// <summary>
/// Standard error response format
/// </summary>
public class ErrorResponseDto
{
    /// <summary>
    /// Error code identifier
    /// </summary>
    public string ErrorCode { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable error message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the error occurred
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Trace ID for correlation
    /// </summary>
    public string? TraceId { get; set; }

    /// <summary>
    /// Additional validation errors if applicable
    /// </summary>
    public Dictionary<string, string[]>? ValidationErrors { get; set; }
}
