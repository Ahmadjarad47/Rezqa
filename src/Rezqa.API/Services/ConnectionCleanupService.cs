using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using Rezqa.API.Hubs;

namespace Rezqa.API.Services
{
    public class ConnectionCleanupService : BackgroundService
    {
        private readonly PresenceTracker _presenceTracker;
        private readonly PresenceMessageTracker _presenceMessageTracker;

        public ConnectionCleanupService(PresenceTracker presenceTracker, PresenceMessageTracker presenceMessageTracker)
        {
            _presenceTracker = presenceTracker;
            _presenceMessageTracker = presenceMessageTracker;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // تنظيف الاتصالات الأقدم من ساعة
                _presenceTracker.CleanupOldConnections(TimeSpan.FromHours(12));
                _presenceMessageTracker.CleanupOldConnections(TimeSpan.FromHours(12));

                await Task.Delay(TimeSpan.FromHours(12), stoppingToken); // انتظر ساعة
            }
        }
    }
}