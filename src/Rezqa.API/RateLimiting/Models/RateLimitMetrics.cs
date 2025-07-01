namespace Rezqa.API.RateLimiting.Models;

public class RateLimitMetrics
{
    public long TotalOperations { get; set; }
    public long TotalErrors { get; set; }
    public Dictionary<string, long> OperationCounts { get; set; } = new();
    public Dictionary<string, long> ErrorCounts { get; set; } = new();
    public Dictionary<string, DateTime> LastAccessTimes { get; set; } = new();
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    public double ErrorRate => TotalOperations > 0 ? (double)TotalErrors / TotalOperations : 0;
    public int ActiveCounters => LastAccessTimes.Count;
    public TimeSpan Uptime => DateTime.UtcNow - LastUpdated;
} 