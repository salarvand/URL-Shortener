using System;

namespace URLShortener.Domain.Events
{
    /// <summary>
    /// Domain event raised when a short URL is clicked
    /// </summary>
    public class UrlClickedEvent : DomainEvent
    {
        public Guid ShortUrlId { get; }
        public string ShortCode { get; }
        public string OriginalUrl { get; }
        public int ClickCount { get; }

        public UrlClickedEvent(Guid shortUrlId, string shortCode, string originalUrl, int clickCount)
        {
            ShortUrlId = shortUrlId;
            ShortCode = shortCode;
            OriginalUrl = originalUrl;
            ClickCount = clickCount;
        }
    }
} 