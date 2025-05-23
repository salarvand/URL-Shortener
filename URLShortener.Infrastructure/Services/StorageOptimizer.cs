using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using URLShortener.Application.Interfaces;
using URLShortener.Domain;
using URLShortener.Domain.Entities;
using URLShortener.Infrastructure.Data;

namespace URLShortener.Infrastructure.Services
{
    public class StorageOptimizer : IStorageOptimizer
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<StorageOptimizer> _logger;
        
        // Constants for storage size estimation
        private const int AVG_URL_SIZE_BYTES = 150; // Average URL size in bytes
        private const int AVG_SHORTCODE_SIZE_BYTES = 10; // Average short code size in bytes
        private const int AVG_CLICK_STAT_SIZE_BYTES = 250; // Average click statistic record size in bytes
        private const int GUID_SIZE_BYTES = 16; // Size of a GUID in bytes
        private const int DATETIME_SIZE_BYTES = 8; // Size of a DateTime in bytes
        private const int INT_SIZE_BYTES = 4; // Size of an int in bytes
        private const int BOOL_SIZE_BYTES = 1; // Size of a bool in bytes

        public StorageOptimizer(AppDbContext dbContext, ILogger<StorageOptimizer> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Purges expired URLs from the database
        /// </summary>
        public async Task<int> PurgeExpiredUrlsAsync()
        {
            try
            {
                // Find expired URLs that are older than their expiration date
                var now = DateTime.UtcNow;
                var expiredUrls = await _dbContext.ShortUrls
                    .Where(u => u.ExpiresAt.HasValue && u.ExpiresAt < now)
                    .ToListAsync();

                _logger.LogInformation("Found {Count} expired URLs to purge", expiredUrls.Count);

                if (expiredUrls.Count == 0)
                    return 0;

                // Before deleting, compress them if they have clicks
                foreach (var url in expiredUrls.Where(u => u.ClickCount > 0))
                {
                    var compressed = CompressedShortUrl.Create(url);
                    await _dbContext.CompressedShortUrls.AddAsync(compressed);
                    
                    _logger.LogDebug("Compressed expired URL {ShortCode} with {ClickCount} clicks", 
                        url.ShortCode, url.ClickCount);
                }

                // Remove the expired URLs
                _dbContext.ShortUrls.RemoveRange(expiredUrls);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Successfully purged {Count} expired URLs", expiredUrls.Count);
                return expiredUrls.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error purging expired URLs");
                throw;
            }
        }

        /// <summary>
        /// Aggregates click statistics for URLs older than the specified threshold
        /// </summary>
        public async Task<int> AggregateOldClickStatisticsAsync(TimeSpan olderThan)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.Subtract(olderThan);
                var recordsAggregated = 0;

                // Get URLs with click statistics older than the threshold
                var urlsWithOldStats = await _dbContext.ShortUrls
                    .Where(u => _dbContext.ClickStatistics
                        .Any(c => c.ShortUrlId == u.Id && c.ClickedAt < cutoffDate))
                    .ToListAsync();

                _logger.LogInformation("Found {Count} URLs with old click statistics", urlsWithOldStats.Count);

                foreach (var url in urlsWithOldStats)
                {
                    // Get old click statistics for this URL
                    var oldStats = await _dbContext.ClickStatistics
                        .Where(c => c.ShortUrlId == url.Id && c.ClickedAt < cutoffDate)
                        .ToListAsync();

                    if (oldStats.Count == 0)
                        continue;

                    // Calculate aggregation period
                    var periodStart = oldStats.Min(c => c.ClickedAt);
                    var periodEnd = oldStats.Max(c => c.ClickedAt);
                    
                    // Create summary data
                    var userAgentSummary = CreateUserAgentSummary(oldStats);
                    var geoSummary = CreateGeographicSummary(oldStats);
                    var refererSummary = CreateRefererSummary(oldStats);

                    // Create aggregated record
                    var aggregated = AggregatedClickStatistic.Create(
                        url.Id,
                        periodStart,
                        periodEnd,
                        oldStats.Count,
                        userAgentSummary,
                        geoSummary,
                        refererSummary
                    );

                    // Add the aggregated record and remove the old individual records
                    await _dbContext.AggregatedClickStatistics.AddAsync(aggregated);
                    _dbContext.ClickStatistics.RemoveRange(oldStats);
                    
                    recordsAggregated += oldStats.Count;
                    
                    _logger.LogDebug("Aggregated {Count} click records for URL {ShortCode}", 
                        oldStats.Count, url.ShortCode);
                }

                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Successfully aggregated {Count} click statistics records", recordsAggregated);
                
                return recordsAggregated;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error aggregating old click statistics");
                throw;
            }
        }

        /// <summary>
        /// Compresses URL data for long-term storage
        /// </summary>
        public async Task<int> CompressOldUrlDataAsync(TimeSpan olderThan)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.Subtract(olderThan);
                
                // Find URLs older than the threshold that haven't been accessed recently
                var oldUrls = await _dbContext.ShortUrls
                    .Where(u => u.CreatedAt < cutoffDate)
                    .Where(u => !_dbContext.ClickStatistics.Any(c => c.ShortUrlId == u.Id && c.ClickedAt > cutoffDate))
                    .ToListAsync();

                _logger.LogInformation("Found {Count} old URLs to compress", oldUrls.Count);

                if (oldUrls.Count == 0)
                    return 0;

                // Compress the URLs
                foreach (var url in oldUrls)
                {
                    var compressed = CompressedShortUrl.Create(url);
                    await _dbContext.CompressedShortUrls.AddAsync(compressed);
                    
                    _logger.LogDebug("Compressed old URL {ShortCode} with {ClickCount} clicks", 
                        url.ShortCode, url.ClickCount);
                }

                // Remove the original URLs
                _dbContext.ShortUrls.RemoveRange(oldUrls);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Successfully compressed {Count} old URLs", oldUrls.Count);
                return oldUrls.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error compressing old URL data");
                throw;
            }
        }

        /// <summary>
        /// Gets current storage usage statistics
        /// </summary>
        public async Task<StorageStatistics> GetStorageStatisticsAsync()
        {
            try
            {
                var now = DateTime.UtcNow;
                
                // Count URLs
                var totalUrls = await _dbContext.ShortUrls.CountAsync();
                var expiredUrls = await _dbContext.ShortUrls
                    .CountAsync(u => u.ExpiresAt.HasValue && u.ExpiresAt < now);
                var activeUrls = totalUrls - expiredUrls;
                
                // Count click statistics
                var totalClickStats = await _dbContext.ClickStatistics.CountAsync();
                var totalAggregatedStats = await _dbContext.AggregatedClickStatistics.CountAsync();
                var totalCompressedUrls = await _dbContext.CompressedShortUrls.CountAsync();
                
                // Estimate storage sizes
                var urlStorageBytes = EstimateUrlStorageSize(totalUrls);
                var clickStatsStorageBytes = EstimateClickStatsStorageSize(totalClickStats, totalAggregatedStats);
                var compressedStorageBytes = EstimateCompressedStorageSize(totalCompressedUrls);
                
                return new StorageStatistics
                {
                    TotalUrls = totalUrls,
                    ActiveUrls = activeUrls,
                    ExpiredUrls = expiredUrls,
                    TotalClickStatistics = totalClickStats,
                    UrlStorageBytes = urlStorageBytes + compressedStorageBytes,
                    ClickStatisticsStorageBytes = clickStatsStorageBytes
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting storage statistics");
                throw;
            }
        }

        #region Helper Methods

        private string CreateUserAgentSummary(List<ClickStatistic> stats)
        {
            var userAgents = stats
                .Where(s => !string.IsNullOrEmpty(s.UserAgent))
                .GroupBy(s => ParseUserAgentType(s.UserAgent))
                .Select(g => new { Browser = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToDictionary(x => x.Browser, x => x.Count);
                
            return JsonSerializer.Serialize(userAgents);
        }

        private string CreateGeographicSummary(List<ClickStatistic> stats)
        {
            // In a real implementation, this would use GeoIP lookup
            // For this example, we'll just count by IP
            var ips = stats
                .Where(s => !string.IsNullOrEmpty(s.IpAddress))
                .GroupBy(s => s.IpAddress)
                .Select(g => new { IP = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10) // Limit to top 10
                .ToDictionary(x => x.IP, x => x.Count);
                
            return JsonSerializer.Serialize(ips);
        }

        private string CreateRefererSummary(List<ClickStatistic> stats)
        {
            var referers = stats
                .Where(s => !string.IsNullOrEmpty(s.RefererUrl))
                .GroupBy(s => ExtractDomain(s.RefererUrl))
                .Select(g => new { Domain = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToDictionary(x => x.Domain, x => x.Count);
                
            return JsonSerializer.Serialize(referers);
        }

        private string ParseUserAgentType(string? userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
                return "Unknown";
                
            userAgent = userAgent.ToLowerInvariant();
            
            if (userAgent.Contains("chrome"))
                return "Chrome";
            if (userAgent.Contains("firefox"))
                return "Firefox";
            if (userAgent.Contains("safari") && !userAgent.Contains("chrome"))
                return "Safari";
            if (userAgent.Contains("edge"))
                return "Edge";
            if (userAgent.Contains("opera"))
                return "Opera";
            if (userAgent.Contains("msie") || userAgent.Contains("trident"))
                return "Internet Explorer";
                
            return "Other";
        }

        private string ExtractDomain(string? url)
        {
            if (string.IsNullOrEmpty(url))
                return "Unknown";
                
            try
            {
                var uri = new Uri(url);
                return uri.Host;
            }
            catch
            {
                return "Invalid URL";
            }
        }

        private long EstimateUrlStorageSize(int urlCount)
        {
            // Estimate size of ShortUrl entities
            // Each URL has: GUID + OriginalUrl + ShortCode + 2 DateTimes + int + bool
            long bytesPerUrl = GUID_SIZE_BYTES + AVG_URL_SIZE_BYTES + AVG_SHORTCODE_SIZE_BYTES + 
                               (2 * DATETIME_SIZE_BYTES) + INT_SIZE_BYTES + BOOL_SIZE_BYTES;
                               
            return urlCount * bytesPerUrl;
        }

        private long EstimateClickStatsStorageSize(int clickStatsCount, int aggregatedStatsCount)
        {
            // Estimate size of ClickStatistic entities
            // Each click stat has: GUID + GUID (ShortUrlId) + DateTime + 3 strings (avg 50 bytes each)
            long bytesPerClickStat = GUID_SIZE_BYTES + GUID_SIZE_BYTES + DATETIME_SIZE_BYTES + (3 * 50);
            
            // Estimate size of AggregatedClickStatistic entities
            // Each aggregated stat has: GUID + GUID + 2 DateTimes + int + 3 JSON strings (avg 200 bytes each) + bool
            long bytesPerAggregatedStat = GUID_SIZE_BYTES + GUID_SIZE_BYTES + (2 * DATETIME_SIZE_BYTES) + 
                                         INT_SIZE_BYTES + (3 * 200) + BOOL_SIZE_BYTES;
            
            return (clickStatsCount * bytesPerClickStat) + (aggregatedStatsCount * bytesPerAggregatedStat);
        }

        private long EstimateCompressedStorageSize(int compressedUrlCount)
        {
            // Estimate size of CompressedShortUrl entities
            // Each compressed URL has: GUID + GUID + ShortCode + compressed data (avg 50 bytes) + 
            // 2 DateTimes + DateTime? + int
            long bytesPerCompressedUrl = GUID_SIZE_BYTES + GUID_SIZE_BYTES + AVG_SHORTCODE_SIZE_BYTES + 
                                        50 + (3 * DATETIME_SIZE_BYTES) + INT_SIZE_BYTES;
                                        
            return compressedUrlCount * bytesPerCompressedUrl;
        }

        #endregion
    }
}