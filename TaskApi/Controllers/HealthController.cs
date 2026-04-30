using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskApi.Models;

namespace TaskApi.Controllers;

/// <summary>
/// Controller for health check endpoints
/// </summary>
[ApiController]
[Route("health")]
public class HealthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<HealthController> _logger;

    public HealthController(AppDbContext context, ILogger<HealthController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Liveness probe - checks if the application is running
    /// </summary>
    [HttpGet("live")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetLiveness()
    {
        return Ok(new { status = "Alive", timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Readiness probe - checks if the application is ready to serve requests
    /// </summary>
    [HttpGet("ready")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetReadiness()
    {
        try
        {
            // Check database connectivity
            await _context.Database.CanConnectAsync();

            return Ok(new
            {
                status = "Ready",
                timestamp = DateTime.UtcNow,
                database = "Connected"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Readiness check failed");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                status = "NotReady",
                timestamp = DateTime.UtcNow,
                database = "Disconnected"
            });
        }
    }
}
