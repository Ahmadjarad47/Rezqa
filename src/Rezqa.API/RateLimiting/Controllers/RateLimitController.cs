using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rezqa.API.RateLimiting.Services;
using Rezqa.API.RateLimiting.Models;
using Rezqa.API.RateLimiting.Options;
using Microsoft.Extensions.Options;

namespace Rezqa.API.RateLimiting.Controllers;

[Route("api/admin/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class RateLimitController : ControllerBase
{
    private readonly IRateLimitMonitoringService _monitoringService;
    private readonly RateLimitSettings _settings;
    private readonly ILogger<RateLimitController> _logger;

    public RateLimitController(
        IRateLimitMonitoringService monitoringService,
        IOptions<RateLimitSettings> settings,
        ILogger<RateLimitController> logger)
    {
        _monitoringService = monitoringService;
        _settings = settings.Value;
        _logger = logger;
    }

    /// <summary>
    /// Get rate limiting metrics and statistics
    /// </summary>
    [HttpGet("metrics")]
    public ActionResult<RateLimitMetrics> GetMetrics()
    {
        try
        {
            var metrics = _monitoringService.GetMetrics();
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving rate limit metrics");
            return StatusCode(500, "Error retrieving rate limit metrics");
        }
    }

    /// <summary>
    /// Get current rate limiting configuration
    /// </summary>
    [HttpGet("configuration")]
    public ActionResult<RateLimitSettings> GetConfiguration()
    {
        try
        {
            return Ok(_settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving rate limit configuration");
            return StatusCode(500, "Error retrieving rate limit configuration");
        }
    }

    /// <summary>
    /// Reset rate limiting metrics
    /// </summary>
    [HttpPost("reset-metrics")]
    public ActionResult ResetMetrics()
    {
        try
        {
            _monitoringService.ResetMetrics();
            _logger.LogInformation("Rate limit metrics have been reset by admin");
            return Ok(new { message = "Rate limit metrics have been reset successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting rate limit metrics");
            return StatusCode(500, "Error resetting rate limit metrics");
        }
    }

    /// <summary>
    /// Get rate limiting health status
    /// </summary>
    [HttpGet("health")]
    public ActionResult GetHealth()
    {
        try
        {
            var metrics = _monitoringService.GetMetrics();
            var healthStatus = new
            {
                status = metrics.ErrorRate < 0.1 ? "Healthy" : "Degraded",
                errorRate = metrics.ErrorRate,
                totalOperations = metrics.TotalOperations,
                totalErrors = metrics.TotalErrors,
                activeCounters = metrics.ActiveCounters,
                uptime = metrics.Uptime,
                lastUpdated = metrics.LastUpdated
            };

            return Ok(healthStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking rate limit health");
            return StatusCode(500, "Error checking rate limit health");
        }
    }
} 