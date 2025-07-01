using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Rezqa.API.RateLimiting.Models;
using System.Collections.Concurrent;

namespace Rezqa.API.RateLimiting.Services;

public class RateLimitMonitoringService : IRateLimitMonitoringService
{
    private readonly ILogger<RateLimitMonitoringService> _logger;
    private readonly ConcurrentDictionary<string, long> _operationCounts;
    private readonly ConcurrentDictionary<string, long> _errorCounts;
    private readonly ConcurrentDictionary<string, DateTime> _lastAccessTimes;
    private readonly object _metricsLock = new object();
    private RateLimitMetrics _metrics;

    public RateLimitMonitoringService(ILogger<RateLimitMonitoringService> logger)
    {
        _logger = logger;
        _operationCounts = new ConcurrentDictionary<string, long>();
        _errorCounts = new ConcurrentDictionary<string, long>();
        _lastAccessTimes = new ConcurrentDictionary<string, DateTime>();
        _metrics = new RateLimitMetrics();
    }

    public void RecordCounterAccess(string id, bool found)
    {
        IncrementOperationCount("CounterAccess");
        IncrementOperationCount(found ? "CounterAccessFound" : "CounterAccessNotFound");
        UpdateLastAccessTime(id);

        _logger.LogDebug("Rate limit counter access recorded for id: {Id}, Found: {Found}", id, found);
    }

    public void RecordCounterSet(string id, object counter)
    {
        IncrementOperationCount("CounterSet");
        UpdateLastAccessTime(id);

        _logger.LogDebug("Rate limit counter set recorded for id: {Id}", id);
    }

    public void RecordCounterRemoval(string id, bool success)
    {
        IncrementOperationCount("CounterRemoval");
        IncrementOperationCount(success ? "CounterRemovalSuccess" : "CounterRemovalFailed");
        UpdateLastAccessTime(id);

        _logger.LogDebug("Rate limit counter removal recorded for id: {Id}, Success: {Success}", id, success);
    }

    public void RecordCounterEviction(string id, EvictionReason reason)
    {
        IncrementOperationCount("CounterEviction");
        IncrementOperationCount($"CounterEviction_{reason}");

        _logger.LogInformation("Rate limit counter evicted for id: {Id}, Reason: {Reason}", id, reason);
    }

    public void RecordExistenceCheck(string id, bool exists)
    {
        IncrementOperationCount("ExistenceCheck");
        IncrementOperationCount(exists ? "ExistenceCheckFound" : "ExistenceCheckNotFound");
        UpdateLastAccessTime(id);

        _logger.LogDebug("Rate limit existence check recorded for id: {Id}, Exists: {Exists}", id, exists);
    }

    public void RecordError(string operation, Exception exception)
    {
        IncrementOperationCount("Errors");
        IncrementErrorCount(operation);

        _logger.LogError(exception, "Rate limit error in operation: {Operation}", operation);
    }

    public RateLimitMetrics GetMetrics()
    {
        lock (_metricsLock)
        {
            _metrics.TotalOperations = _operationCounts.Values.Sum();
            _metrics.TotalErrors = _errorCounts.Values.Sum();
            _metrics.OperationCounts = new Dictionary<string, long>(_operationCounts);
            _metrics.ErrorCounts = new Dictionary<string, long>(_errorCounts);
            _metrics.LastAccessTimes = new Dictionary<string, DateTime>(_lastAccessTimes);
            _metrics.LastUpdated = DateTime.UtcNow;

            return _metrics;
        }
    }

    public void ResetMetrics()
    {
        lock (_metricsLock)
        {
            _operationCounts.Clear();
            _errorCounts.Clear();
            _lastAccessTimes.Clear();
            _metrics = new RateLimitMetrics();

            _logger.LogInformation("Rate limit metrics have been reset");
        }
    }

    private void IncrementOperationCount(string operation)
    {
        _operationCounts.AddOrUpdate(operation, 1, (key, value) => value + 1);
    }

    private void IncrementErrorCount(string operation)
    {
        _errorCounts.AddOrUpdate(operation, 1, (key, value) => value + 1);
    }

    private void UpdateLastAccessTime(string id)
    {
        _lastAccessTimes.AddOrUpdate(id, DateTime.UtcNow, (key, value) => DateTime.UtcNow);
    }
}