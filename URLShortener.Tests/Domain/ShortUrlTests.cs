using System;
using URLShortener.Domain;
using URLShortener.Domain.Entities;
using URLShortener.Tests.Common;

namespace URLShortener.Tests.Domain
{
    public class ShortUrlTests
    {
        [Fact]
        public void Create_WithValidParameters_ReturnsShortUrl()
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
            shortUrl.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            shortUrl.ClickCount.Should().Be(0);
            shortUrl.IsActive.Should().BeTrue();
            shortUrl.ExpiresAt.Should().BeNull();
        }
        
        [Fact]
        public void Create_WithExpiryDate_SetsExpiryDate()
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
        
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void Create_WithInvalidOriginalUrl_ThrowsArgumentException(string invalidUrl)
        {
            // Arrange
            var shortCode = "abc123";
            
            // Act & Assert
            Action act = () => ShortUrl.Create(invalidUrl, shortCode);
            act.Should().Throw<ArgumentException>()
                .WithMessage("Original URL cannot be null or empty*");
        }
        
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void Create_WithInvalidShortCode_ThrowsArgumentException(string invalidShortCode)
        {
            // Arrange
            var originalUrl = "https://example.com";
            
            // Act & Assert
            Action act = () => ShortUrl.Create(originalUrl, invalidShortCode);
            act.Should().Throw<ArgumentException>()
                .WithMessage("Short code cannot be null or empty*");
        }
        
        [Fact]
        public void RecordClick_IncrementsClickCount()
        {
            // Arrange
            var shortUrl = TestDataFactory.CreateValidShortUrl(clickCount: 5);
            
            // Act
            shortUrl.RecordClick();
            
            // Assert
            shortUrl.ClickCount.Should().Be(6);
        }
        
        [Fact]
        public void Deactivate_SetsIsActiveToFalse()
        {
            // Arrange
            var shortUrl = TestDataFactory.CreateValidShortUrl(isActive: true);
            
            // Act
            shortUrl.Deactivate();
            
            // Assert
            shortUrl.IsActive.Should().BeFalse();
        }
        
        [Fact]
        public void SetExpiryDate_UpdatesExpiryDate()
        {
            // Arrange
            var shortUrl = TestDataFactory.CreateValidShortUrl();
            var newExpiryDate = DateTime.UtcNow.AddDays(30);
            
            // Act
            shortUrl.SetExpiryDate(newExpiryDate);
            
            // Assert
            shortUrl.ExpiresAt.Should().BeCloseTo(newExpiryDate, TimeSpan.FromSeconds(1));
        }
        
        [Fact]
        public void IsExpired_WhenExpiryDateInPast_ReturnsTrue()
        {
            // Arrange
            var expiredDate = DateTime.UtcNow.AddDays(-1);
            var shortUrl = TestDataFactory.CreateValidShortUrl(expiresAt: expiredDate);
            
            // Act
            var result = shortUrl.IsExpired();
            
            // Assert
            result.Should().BeTrue();
        }
        
        [Fact]
        public void IsExpired_WhenExpiryDateInFuture_ReturnsFalse()
        {
            // Arrange
            var futureDate = DateTime.UtcNow.AddDays(1);
            var shortUrl = TestDataFactory.CreateValidShortUrl(expiresAt: futureDate);
            
            // Act
            var result = shortUrl.IsExpired();
            
            // Assert
            result.Should().BeFalse();
        }
        
        [Fact]
        public void IsExpired_WhenNoExpiryDate_ReturnsFalse()
        {
            // Arrange
            var shortUrl = TestDataFactory.CreateValidShortUrl(expiresAt: null);
            
            // Act
            var result = shortUrl.IsExpired();
            
            // Assert
            result.Should().BeFalse();
        }
    }
} 