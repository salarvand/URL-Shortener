using System;

namespace URLShortener.Domain.Entities
{
    /// <summary>
    /// Entity representing a single URL click event
    /// </summary>
    public class ClickStatistic : Entity
    {
        public Guid ShortUrlId { get; private set; }
        public DateTime ClickedAt { get; private set; }
        public string? UserAgent { get; private set; }
        public string? IpAddress { get; private set; }
        public string? RefererUrl { get; private set; }

        protected ClickStatistic() { }

        public static ClickStatistic Create(Guid shortUrlId, string? userAgent = null, string? ipAddress = null, string? refererUrl = null)
        {
            var clickStat = new ClickStatistic
            {
                Id = Guid.NewGuid(),
                ShortUrlId = shortUrlId,
                ClickedAt = DateTime.UtcNow,
                UserAgent = userAgent,
                IpAddress = ipAddress,
                RefererUrl = refererUrl
            };
            
            return clickStat;
        }
    }
} 