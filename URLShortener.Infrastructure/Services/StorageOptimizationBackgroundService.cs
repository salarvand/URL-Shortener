using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using URLShortener.Application.Interfaces;

namespace URLShortener.Infrastructure.Services
{
    /// <summary>
    /// Background service that periodically runs storage optimization tasks
    /// </summary>
    public class StorageOptimizationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<StorageOptimizationBackgroundService> _logger;
        
        // Default intervals for optimization tasks
        private static readonly TimeSpan _purgeInterval = TimeSpan.FromHours(24); // Run once per day
        private static readonly TimeSpan _aggregationInterval = TimeSpan.FromDays(7); // Run once per week
        private static readonly TimeSpan _compressionInterval = TimeSpan.FromDays(30); // Run once per month
        
        // Thresholds for optimization
        private static readonly TimeSpan _clickAggregationAge = TimeSpan.FromDays(30); // Aggregate clicks older than 30 days
        private static readonly TimeSpan _urlCompressionAge = TimeSpan.FromDays(90); // Compress URLs older than 90 days

        public StorageOptimizationBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<StorageOptimizationBackgroundService> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Storage optimization background service is starting");

            // Initialize timers
            using var purgeTimer = new PeriodicTimer(_purgeInterval);
            using var aggregationTimer = new PeriodicTimer(_aggregationInterval);
            using var compressionTimer = new PeriodicTimer(_compressionInterval);
            
            // Run initial optimization on startup
            await RunPurgeTask(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                // Run purge task when timer elapses
                if (await purgeTimer.WaitForNextTickAsync(stoppingToken))
                {
                    await RunPurgeTask(stoppingToken);
                }
                
                // Run aggregation task when timer elapses
                if (await aggregationTimer.WaitForNextTickAsync(stoppingToken))
                {
                    await RunAggregationTask(stoppingToken);
                }
                
                // Run compression task when timer elapses
                if (await compressionTimer.WaitForNextTickAsync(stoppingToken))
                {
                    await RunCompressionTask(stoppingToken);
                }
            }

            _logger.LogInformation("Storage optimization background service is stopping");
        }

        private async Task RunPurgeTask(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Running expired URL purge task");
                
                using var scope = _serviceProvider.CreateScope();
                var storageOptimizer = scope.ServiceProvider.GetRequiredService<IStorageOptimizer>();
                
                var purgedCount = await storageOptimizer.PurgeExpiredUrlsAsync();
                _logger.LogInformation("Purged {Count} expired URLs", purgedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running purge task");
            }
        }

        private async Task RunAggregationTask(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Running click statistics aggregation task");
                
                using var scope = _serviceProvider.CreateScope();
                var storageOptimizer = scope.ServiceProvider.GetRequiredService<IStorageOptimizer>();
                
                var aggregatedCount = await storageOptimizer.AggregateOldClickStatisticsAsync(_clickAggregationAge);
                _logger.LogInformation("Aggregated {Count} click statistics", aggregatedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running aggregation task");
            }
        }

        private async Task RunCompressionTask(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Running URL compression task");
                
                using var scope = _serviceProvider.CreateScope();
                var storageOptimizer = scope.ServiceProvider.GetRequiredService<IStorageOptimizer>();
                
                var compressedCount = await storageOptimizer.CompressOldUrlDataAsync(_urlCompressionAge);
                _logger.LogInformation("Compressed {Count} old URLs", compressedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running compression task");
            }
        }
    }
} 