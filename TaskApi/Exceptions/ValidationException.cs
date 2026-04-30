namespace TaskApi.Exceptions;

/// <summary>
/// Exception thrown when validation fails
/// </summary>
public class ValidationException : Exception
{
    public Dictionary<string, string[]> ValidationErrors { get; }

    public ValidationException(string message, Dictionary<string, string[]> validationErrors)
        : base(message)
    {
        ValidationErrors = validationErrors;
    }

    public ValidationException(string message) : base(message)
    {
        ValidationErrors = new Dictionary<string, string[]>();
    }
}
