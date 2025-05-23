using System;

namespace URLShortener.Domain
{
    public class ShortUrl
    {
        public Guid Id { get; private set; }
        public string OriginalUrl { get; private set; }
        public string ShortCode { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? ExpiresAt { get; private set; }
        public int ClickCount { get; private set; }
        public bool IsActive { get; private set; }

        private ShortUrl() { }

        public ShortUrl(string originalUrl, string shortCode, DateTime? expiresAt = null)
        {
            if (string.IsNullOrWhiteSpace(originalUrl))
                throw new ArgumentException("Original URL cannot be empty", nameof(originalUrl));
            
            if (string.IsNullOrWhiteSpace(shortCode))
                throw new ArgumentException("Short code cannot be empty", nameof(shortCode));

            Id = Guid.NewGuid();
            OriginalUrl = originalUrl;
            ShortCode = shortCode;
            CreatedAt = DateTime.UtcNow;
            ExpiresAt = expiresAt;
            ClickCount = 0;
            IsActive = true;
        }

        public void IncrementClickCount()
        {
            ClickCount++;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        public bool IsExpired()
        {
            return ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;
        }
    }
} 