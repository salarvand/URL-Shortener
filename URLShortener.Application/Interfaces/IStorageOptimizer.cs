using System;
using System.Threading.Tasks;

namespace URLShortener.Application.Interfaces
{
    /// <summary>
    /// Interface for optimizing storage usage in the URL shortener
    /// </summary>
    public interface IStorageOptimizer
    {
        /// <summary>
        /// Purges expired URLs from the database
        /// </summary>
        /// <returns>Number of records purged</returns>
        Task<int> PurgeExpiredUrlsAsync();
        
        /// <summary>
        /// Aggregates click statistics for URLs older than the specified threshold
        /// </summary>
        /// <param name="olderThan">Age threshold for aggregation</param>
        /// <returns>Number of records aggregated</returns>
        Task<int> AggregateOldClickStatisticsAsync(TimeSpan olderThan);
        
        /// <summary>
        /// Compresses URL data for long-term storage
        /// </summary>
        /// <param name="olderThan">Age threshold for compression</param>
        /// <returns>Number of records compressed</returns>
        Task<int> CompressOldUrlDataAsync(TimeSpan olderThan);
        
        /// <summary>
        /// Gets current storage usage statistics
        /// </summary>
        /// <returns>Storage statistics</returns>
        Task<StorageStatistics> GetStorageStatisticsAsync();
    }
    
    /// <summary>
    /// Storage statistics data
    /// </summary>
    public class StorageStatistics
    {
        /// <summary>
        /// Total number of short URLs in the system
        /// </summary>
        public int TotalUrls { get; set; }
        
        /// <summary>
        /// Number of active (non-expired) short URLs
        /// </summary>
        public int ActiveUrls { get; set; }
        
        /// <summary>
        /// Number of expired short URLs
        /// </summary>
        public int ExpiredUrls { get; set; }
        
        /// <summary>
        /// Total number of click statistics records
        /// </summary>
        public int TotalClickStatistics { get; set; }
        
        /// <summary>
        /// Estimated storage space used by URLs (in bytes)
        /// </summary>
        public long UrlStorageBytes { get; set; }
        
        /// <summary>
        /// Estimated storage space used by click statistics (in bytes)
        /// </summary>
        public long ClickStatisticsStorageBytes { get; set; }
        
        /// <summary>
        /// Total estimated storage space used (in bytes)
        /// </summary>
        public long TotalStorageBytes => UrlStorageBytes + ClickStatisticsStorageBytes;
    }
} 