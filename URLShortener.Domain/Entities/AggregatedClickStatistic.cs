using System;

namespace URLShortener.Domain.Entities
{
    /// <summary>
    /// Entity representing aggregated click statistics for a URL over a time period
    /// </summary>
    public class AggregatedClickStatistic : Entity
    {
        public Guid ShortUrlId { get; private set; }
        public DateTime PeriodStart { get; private set; }
        public DateTime PeriodEnd { get; private set; }
        public int ClickCount { get; private set; }
        
        // Most common user agents (browsers) - stored as JSON
        public string? UserAgentSummary { get; private set; }
        
        // Geographic distribution based on IP addresses - stored as JSON
        public string? GeographicSummary { get; private set; }
        
        // Referrer distribution - stored as JSON
        public string? RefererSummary { get; private set; }
        
        // Compression flag
        public bool IsCompressed { get; private set; }

        protected AggregatedClickStatistic() { }

        public static AggregatedClickStatistic Create(
            Guid shortUrlId, 
            DateTime periodStart, 
            DateTime periodEnd, 
            int clickCount,
            string? userAgentSummary = null,
            string? geographicSummary = null,
            string? refererSummary = null)
        {
            if (periodEnd <= periodStart)
                throw new ArgumentException("Period end must be after period start");
                
            if (clickCount < 0)
                throw new ArgumentException("Click count cannot be negative");

            var aggregatedStat = new AggregatedClickStatistic
            {
                Id = Guid.NewGuid(),
                ShortUrlId = shortUrlId,
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                ClickCount = clickCount,
                UserAgentSummary = userAgentSummary,
                GeographicSummary = geographicSummary,
                RefererSummary = refererSummary,
                IsCompressed = false
            };
            
            return aggregatedStat;
        }
        
        public void SetCompressed(bool compressed)
        {
            IsCompressed = compressed;
        }
        
        public void AddClicks(int additionalClicks)
        {
            if (additionalClicks < 0)
                throw new ArgumentException("Additional clicks cannot be negative");
                
            ClickCount += additionalClicks;
        }
    }
} 