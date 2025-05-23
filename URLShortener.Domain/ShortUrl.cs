using System;

namespace URLShortener.Domain
{
    public class ShortUrl
    {
        // Private setters protect domain invariants but allow EF Core to set values
        public Guid Id { get; private set; }
        public string OriginalUrl { get; private set; }
        public string ShortCode { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? ExpiresAt { get; private set; }
        public int ClickCount { get; private set; }
        public bool IsActive { get; private set; }

        // Protected constructor for EF Core
        protected ShortUrl() { }

        // Factory method that enforces business rules during creation
        public static ShortUrl Create(string originalUrl, string shortCode, DateTime? expiresAt = null)
        {
            if (string.IsNullOrWhiteSpace(originalUrl))
                throw new ArgumentException("Original URL cannot be empty", nameof(originalUrl));
            
            if (string.IsNullOrWhiteSpace(shortCode))
                throw new ArgumentException("Short code cannot be empty", nameof(shortCode));

            return new ShortUrl
            {
                Id = Guid.NewGuid(),
                OriginalUrl = originalUrl,
                ShortCode = shortCode,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt,
                ClickCount = 0,
                IsActive = true
            };
        }

        // Factory method to reconstitute existing entities (for persistence/testing)
        public static ShortUrl Reconstitute(
            Guid id, 
            string originalUrl, 
            string shortCode, 
            DateTime createdAt, 
            int clickCount, 
            bool isActive,
            DateTime? expiresAt = null)
        {
            if (string.IsNullOrWhiteSpace(originalUrl))
                throw new ArgumentException("Original URL cannot be empty", nameof(originalUrl));
            
            if (string.IsNullOrWhiteSpace(shortCode))
                throw new ArgumentException("Short code cannot be empty", nameof(shortCode));

            return new ShortUrl
            {
                Id = id,
                OriginalUrl = originalUrl,
                ShortCode = shortCode,
                CreatedAt = createdAt,
                ExpiresAt = expiresAt,
                ClickCount = clickCount,
                IsActive = isActive
            };
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