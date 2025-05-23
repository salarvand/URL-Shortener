using System;
using FluentAssertions;
using URLShortener.Domain;
using URLShortener.Domain.Entities;

namespace URLShortener.Tests.Domain
{
    public class BasicEntityTests
    {
        [Fact]
        public void ShortUrl_Create_SetsPropertiesCorrectly()
        {
            // Arrange
            var originalUrl = "https://example.com";
            var shortCode = "abc123";
            
            // Act
            var shortUrl = ShortUrl.Create(originalUrl, shortCode);
            
            // Assert
            shortUrl.Should().NotBeNull();
            shortUrl.OriginalUrl.Should().Be(originalUrl);
            shortUrl.ShortCode.Should().Be(shortCode);
            shortUrl.ClickCount.Should().Be(0);
            shortUrl.IsActive.Should().BeTrue();
            shortUrl.ExpiresAt.Should().BeNull();
        }
        
        [Fact]
        public void ShortUrl_RecordClick_IncrementsClickCount()
        {
            // Arrange
            var originalUrl = "https://example.com";
            var shortCode = "abc123";
            var shortUrl = ShortUrl.Create(originalUrl, shortCode);
            
            // Act
            shortUrl.RecordClick();
            
            // Assert
            shortUrl.ClickCount.Should().Be(1);
        }
        
        [Fact]
        public void ShortUrl_Deactivate_SetsIsActiveToFalse()
        {
            // Arrange
            var originalUrl = "https://example.com";
            var shortCode = "abc123";
            var shortUrl = ShortUrl.Create(originalUrl, shortCode);
            
            // Act
            shortUrl.Deactivate();
            
            // Assert
            shortUrl.IsActive.Should().BeFalse();
        }

        [Fact]
        public void ShortUrl_WithExpiryDate_SetsExpiryDateProperty()
        {
            // Arrange
            var originalUrl = "https://example.com";
            var shortCode = "abc123";
            var expiryDate = DateTime.UtcNow.AddDays(7);
            
            // Act
            var shortUrl = ShortUrl.Create(originalUrl, shortCode, expiryDate);
            
            // Assert
            shortUrl.ExpiresAt.Should().BeCloseTo(expiryDate, TimeSpan.FromSeconds(1));
        }
        
        [Fact]
        public void ShortUrl_IsExpired_ReturnsTrueForPastDate()
        {
            // Arrange
            var originalUrl = "https://example.com";
            var shortCode = "abc123";
            // Use reconstitute to bypass validation that prevents creating with past date
            var shortUrl = ShortUrl.Reconstitute(
                Guid.NewGuid(),
                originalUrl,
                shortCode,
                DateTime.UtcNow.AddDays(-10),
                0,
                true,
                DateTime.UtcNow.AddDays(-1) // Past date
            );
            
            // Act
            var result = shortUrl.IsExpired();
            
            // Assert
            result.Should().BeTrue();
        }
        
        [Fact]
        public void ShortUrl_IsExpired_ReturnsFalseForFutureDate()
        {
            // Arrange
            var originalUrl = "https://example.com";
            var shortCode = "abc123";
            var shortUrl = ShortUrl.Create(originalUrl, shortCode, DateTime.UtcNow.AddDays(1)); // Future date
            
            // Act
            var result = shortUrl.IsExpired();
            
            // Assert
            result.Should().BeFalse();
        }
    }
} 