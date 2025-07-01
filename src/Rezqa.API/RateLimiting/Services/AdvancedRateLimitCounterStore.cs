using AspNetCoreRateLimit;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rezqa.API.RateLimiting.Options;
using System;
using System.Threading.Tasks;

namespace Rezqa.API.RateLimiting.Services;

public class AdvancedRateLimitCounterStore : IRateLimitCounterStore
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<AdvancedRateLimitCounterStore> _logger;
    private readonly RateLimitSettings _settings;
    private readonly IRateLimitMonitoringService _monitoringService;

    public AdvancedRateLimitCounterStore(
        ILogger<AdvancedRateLimitCounterStore> logger,
        IOptions<RateLimitSettings> settings,
        IRateLimitMonitoringService monitoringService)
    {
        _logger = logger;
        _settings = settings.Value;
        _monitoringService = monitoringService;

        // Create a separate memory cache instance optimized for rate limiting
        _cache = new MemoryCache(new MemoryCacheOptions
        {
            ExpirationScanFrequency = TimeSpan.FromMinutes(5),
            CompactionPercentage = 0.25
        });
    }

    public Task<RateLimitCounter?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_cache.TryGetValue(id, out var counter))
            {
                var rateLimitCounter = counter as RateLimitCounter?;

                if (_settings.EnableRateLimitLogging)
                {
                    _logger.LogDebug("Retrieved rate limit counter for id: {Id}, Count: {Count}",
                        id, rateLimitCounter?.Count);
                }

                _monitoringService.RecordCounterAccess(id, true);
                return Task.FromResult(rateLimitCounter);
            }

            _monitoringService.RecordCounterAccess(id, false);
            return Task.FromResult<RateLimitCounter?>(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting rate limit counter for id: {Id}", id);
            _monitoringService.RecordError("GetAsync", ex);
            return Task.FromResult<RateLimitCounter?>(null);
        }
    }

    public Task RemoveAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            _cache.Remove(id);

            if (_settings.EnableRateLimitLogging)
            {
                _logger.LogDebug("Removed rate limit counter for id: {Id}", id);
            }

            _monitoringService.RecordCounterRemoval(id, true);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing rate limit counter for id: {Id}", id);
            _monitoringService.RecordError("RemoveAsync", ex);
            return Task.CompletedTask;
        }
    }

    public Task SetAsync(string id, RateLimitCounter? counter, TimeSpan? expirationTime = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (counter == null)
            {
                _cache.Remove(id);

                if (_settings.EnableRateLimitLogging)
                {
                    _logger.LogDebug("Set null counter - removed rate limit counter for id: {Id}", id);
                }

                _monitoringService.RecordCounterRemoval(id, true);
                return Task.CompletedTask;
            }

            var options = new MemoryCacheEntryOptions();

            if (expirationTime.HasValue)
            {
                options.AbsoluteExpirationRelativeToNow = expirationTime;
            }
            else
            {
                // Default expiration based on the rate limit period
                options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            }

            // Set a default size to avoid the SizeLimit error
            options.Size = 1;

            // Add callback for monitoring
            options.RegisterPostEvictionCallback((object? key, object? value, EvictionReason reason, object? state) =>
            {
                if (_settings.EnableRateLimitLogging)
                {
                    _logger.LogDebug("Rate limit counter evicted for id: {Id}, Reason: {Reason}", key, reason);
                }
                _monitoringService.RecordCounterEviction(key?.ToString() ?? "", reason);
            });

            _cache.Set(id, counter, options);

            if (_settings.EnableRateLimitLogging)
            {
                _logger.LogDebug("Set rate limit counter for id: {Id}, Count: {Count}", id);
            }

            _monitoringService.RecordCounterSet(id, counter);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting rate limit counter for id: {Id}", id);
            _monitoringService.RecordError("SetAsync", ex);
            return Task.CompletedTask;
        }
    }

    public Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var exists = _cache.TryGetValue(id, out _);

            if (_settings.EnableRateLimitLogging)
            {
                _logger.LogDebug("Checked existence of rate limit counter for id: {Id}, Exists: {Exists}", id, exists);
            }

            _monitoringService.RecordExistenceCheck(id, exists);
            return Task.FromResult(exists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if rate limit counter exists for id: {Id}", id);
            _monitoringService.RecordError("ExistsAsync", ex);
            return Task.FromResult(false);
        }
    }
}