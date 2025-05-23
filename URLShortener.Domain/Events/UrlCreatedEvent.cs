using System;

namespace URLShortener.Domain.Events
{
    /// <summary>
    /// Domain event raised when a new short URL is created
    /// </summary>
    public class UrlCreatedEvent : DomainEvent
    {
        public Guid ShortUrlId { get; }
        public string ShortCode { get; }
        public string OriginalUrl { get; }
        public DateTime? ExpiresAt { get; }

        public UrlCreatedEvent(Guid shortUrlId, string shortCode, string originalUrl, DateTime? expiresAt)
        {
            ShortUrlId = shortUrlId;
            ShortCode = shortCode;
            OriginalUrl = originalUrl;
            ExpiresAt = expiresAt;
        }
    }
} 