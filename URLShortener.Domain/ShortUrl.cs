using System;
using System.Collections.Generic;
using URLShortener.Domain.Entities;
using URLShortener.Domain.Events;
using URLShortener.Domain.Validators;
using URLShortener.Domain.ValueObjects;

namespace URLShortener.Domain
{
    /// <summary>
    /// Aggregate root for shortened URLs
    /// </summary>
    public class ShortUrl : Entity, IAggregateRoot
    {
        // Backing fields - these will store the actual values
        private string _originalUrl;
        private string _shortCode;
        
        // Properties for domain logic - create value objects on-the-fly when accessed
        public string OriginalUrl 
        {
            get => _originalUrl;
            // Private setter for EF Core
            private set => _originalUrl = value;
        }
        
        public string ShortCode 
        {
            get => _shortCode;
            // Private setter for EF Core
            private set => _shortCode = value;
        }
        
        public DateTime CreatedAt { get; private set; }
        public DateTime? ExpiresAt { get; private set; }
        public int ClickCount { get; private set; }
        public bool IsActive { get; private set; }
        
        // Navigation properties
        private readonly List<ClickStatistic> _clickStatistics;
        public IReadOnlyCollection<ClickStatistic> ClickStatistics => _clickStatistics?.AsReadOnly();

        // Protected constructor for EF Core
        protected ShortUrl() 
        {
            _clickStatistics = new List<ClickStatistic>();
        }

        // Factory method that enforces business rules during creation
        public static ShortUrl Create(string originalUrl, string shortCode, DateTime? expiresAt = null)
        {
            // Validate inputs using domain validators
            if (!ShortUrlValidator.IsValidUrl(originalUrl))
                throw new ArgumentException("Invalid URL format", nameof(originalUrl));
                
            if (!ShortUrlValidator.IsValidShortCode(shortCode))
                throw new ArgumentException("Invalid short code format", nameof(shortCode));
                
            if (expiresAt.HasValue && !ShortUrlValidator.IsValidExpirationDate(expiresAt))
                throw new ArgumentException("Expiration date must be in the future", nameof(expiresAt));

            // Create the entity
            var shortUrl = new ShortUrl();
            
            shortUrl.Id = Guid.NewGuid();
            shortUrl._originalUrl = originalUrl;
            shortUrl._shortCode = shortCode;
            shortUrl.CreatedAt = DateTime.UtcNow;
            shortUrl.ExpiresAt = expiresAt;
            shortUrl.ClickCount = 0;
            shortUrl.IsActive = true;

            // Add domain event
            shortUrl.AddDomainEvent(new UrlCreatedEvent(
                shortUrl.Id, 
                shortUrl.ShortCode, 
                shortUrl.OriginalUrl, 
                expiresAt));

            return shortUrl;
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
            // Validate inputs - less strict for reconstitution since we're loading from DB
            if (string.IsNullOrWhiteSpace(originalUrl))
                throw new ArgumentException("Original URL cannot be empty", nameof(originalUrl));
                
            if (string.IsNullOrWhiteSpace(shortCode))
                throw new ArgumentException("Short code cannot be empty", nameof(shortCode));

            var shortUrl = new ShortUrl();
            
            shortUrl.Id = id;
            shortUrl._originalUrl = originalUrl;
            shortUrl._shortCode = shortCode;
            shortUrl.CreatedAt = createdAt;
            shortUrl.ExpiresAt = expiresAt;
            shortUrl.ClickCount = clickCount;
            shortUrl.IsActive = isActive;

            return shortUrl;
        }

        public void IncrementClickCount()
        {
            ClickCount++;
            
            // Add domain event
            AddDomainEvent(new UrlClickedEvent(
                Id, 
                ShortCode, 
                OriginalUrl, 
                ClickCount));
        }

        public void RecordClick(string? userAgent = null, string? ipAddress = null, string? refererUrl = null)
        {
            var clickStat = ClickStatistic.Create(Id, userAgent, ipAddress, refererUrl);
            _clickStatistics.Add(clickStat);
            IncrementClickCount();
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