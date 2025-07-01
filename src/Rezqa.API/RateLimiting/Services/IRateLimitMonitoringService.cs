using Microsoft.Extensions.Caching.Memory;
using Rezqa.API.RateLimiting.Models;

namespace Rezqa.API.RateLimiting.Services;

public interface IRateLimitMonitoringService
{
    void RecordCounterAccess(string id, bool found);
    void RecordCounterSet(string id, object counter);
    void RecordCounterRemoval(string id, bool success);
    void RecordCounterEviction(string id, EvictionReason reason);
    void RecordExistenceCheck(string id, bool exists);
    void RecordError(string operation, Exception exception);
    RateLimitMetrics GetMetrics();
    void ResetMetrics();
}