using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rezqa.Application.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Rezqa.API.Services
{
    public class AdExpirationService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AdExpirationService> _logger;
        private const int ExpirationDays = 60;
        private static readonly TimeSpan Interval = TimeSpan.FromHours(12);

        public AdExpirationService(IServiceScopeFactory scopeFactory, ILogger<AdExpirationService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var adRepository = scope.ServiceProvider.GetRequiredService<IAdRepository>();
                        int count = await adRepository.DeactivateExpiredAdsAsync(ExpirationDays, stoppingToken);
                        if (count > 0)
                            _logger.LogInformation($"AdExpirationService: Deactivated {count} expired ads.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "AdExpirationService: Error deactivating expired ads.");
                }
                await Task.Delay(Interval, stoppingToken);
            }
        }
    }
} 