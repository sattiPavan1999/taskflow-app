using System.Diagnostics;
using System.Net;
using System.Text.Json;
using TaskApi.DTOs;
using TaskApi.Exceptions;

namespace TaskApi.Middleware;

/// <summary>
/// Global exception handling middleware
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;

        _logger.LogError(exception, "An error occurred: {Message}", exception.Message);

        var errorResponse = new ErrorResponseDto
        {
            Timestamp = DateTime.UtcNow,
            TraceId = traceId
        };

        switch (exception)
        {
            case NotFoundException notFoundEx:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse.ErrorCode = "NOT_FOUND";
                errorResponse.Message = notFoundEx.Message;
                break;

            case ValidationException validationEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.ErrorCode = "VALIDATION_ERROR";
                errorResponse.Message = validationEx.Message;
                errorResponse.ValidationErrors = validationEx.ValidationErrors;
                break;

            case ArgumentException argEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.ErrorCode = "INVALID_ARGUMENT";
                errorResponse.Message = argEx.Message;
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse.ErrorCode = "INTERNAL_SERVER_ERROR";
                errorResponse.Message = "An unexpected error occurred. Please try again later.";
                break;
        }

        context.Response.ContentType = "application/json";

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse, jsonOptions));
    }
}
