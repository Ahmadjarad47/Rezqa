using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using System.Collections.Generic;

namespace Rezqa.API.Controllers.Admin
{
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class ReportController : BaseController
    {
        private readonly IMemoryCache _memoryCache;
        private static readonly DateTime _processStartTime = Process.GetCurrentProcess().StartTime.ToUniversalTime();

        public ReportController(MediatR.IMediator mediator, IMemoryCache memoryCache) : base(mediator)
        {
            _memoryCache = memoryCache;
        }

        /// <summary>
        /// Get basic application status
        /// </summary>
        [HttpGet("status")]
        public IActionResult GetApplicationStatus()
        {
            var status = new
            {
                status = "Running",
                timestamp = System.DateTime.UtcNow,
                version = "1.0.0"
            };

            return Ok(status);
        }

        /// <summary>
        /// Get application health and memory usage
        /// </summary>
        [HttpGet("health")]
        public IActionResult GetHealth()
        {
            var process = Process.GetCurrentProcess();
            var health = new
            {
                status = "Healthy",
                timestamp = System.DateTime.UtcNow,
                uptime = (System.DateTime.UtcNow - _processStartTime).ToString(),
                ram_used_mb = process.WorkingSet64 / (1024 * 1024),
                managed_memory_mb = GC.GetTotalMemory(false) / (1024 * 1024),
                // Note: IMemoryCache does not expose total size, only count if using custom implementation
                cache_count = GetCacheCount(),
                version = "1.0.0"
            };
            return Ok(health);
        }

        /// <summary>
        /// Get detailed application metrics
        /// </summary>
        [HttpGet("metrics")]
        public IActionResult GetMetrics()
        {
            var process = Process.GetCurrentProcess();
            var metrics = new
            {
                status = "Running",
                timestamp = System.DateTime.UtcNow,
                uptime = (System.DateTime.UtcNow - _processStartTime).ToString(),
                ram_used_mb = process.WorkingSet64 / (1024 * 1024),
                managed_memory_mb = GC.GetTotalMemory(false) / (1024 * 1024),
                threads = process.Threads.Count,
                handle_count = process.HandleCount,
                process_id = process.Id,
                cache_count = GetCacheCount().ToString(),
                version = "1.0.0"
            };
            return Ok(metrics);
        }

        /// <summary>
        /// Clear all cache entries
        /// </summary>
        [HttpPost("clear-cache")]
        public IActionResult ClearAllCache()
        {
            var keysToRemove = new[]
            {
        "AllAdsCacheKey",
        "AllCategories",
        "AllDynamicFields",
        "AllSubCategories"
    };

            foreach (var key in keysToRemove)
            {
                _memoryCache.Remove(key);
            }

            return Ok(new
            {
                message = "Selected cache keys cleared.",
                keysCleared = keysToRemove
            });
        }

        // Helper to get cache count (works only if IMemoryCache is MemoryCache)
        private int? GetCacheCount()
        {
            if (_memoryCache is MemoryCache memCache)
            {
                var entriesCollection = memCache.GetType().GetProperty("EntriesCollection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(memCache);
                if (entriesCollection is System.Collections.ICollection coll)
                {
                    return coll.Count;
                }
            }
            return null;
        }
    }
}