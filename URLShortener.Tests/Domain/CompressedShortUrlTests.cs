using System;
using URLShortener.Domain;
using URLShortener.Domain.Entities;
using URLShortener.Tests.Common;

namespace URLShortener.Tests.Domain
{
    public class CompressedShortUrlTests
    {
        [Fact]
        public void Create_WithValidShortUrl_ReturnsCompressedShortUrl()
        {
            // Arrange
            var originalUrl = "https://example.com/very/long/path/to/some/resource/that/will/be/compressed";
            var shortCode = "abc123";
            var createdAt = DateTime.UtcNow.AddDays(-100);
            var expiresAt = DateTime.UtcNow.AddDays(30);
            var clickCount = 500;
            
            var shortUrl = ShortUrl.Reconstitute(
                Guid.NewGuid(),
                originalUrl,
                shortCode,
                createdAt,
                clickCount,
                true,
                expiresAt
            );
            
            // Act
            var compressedUrl = CompressedShortUrl.Create(shortUrl);
            
            // Assert
            compressedUrl.Should().NotBeNull();
            compressedUrl.Id.Should().NotBeEmpty();
            compressedUrl.OriginalId.Should().Be(shortUrl.Id);
            compressedUrl.ShortCode.Should().Be(shortCode);
            compressedUrl.CompressedData.Should().NotBeEmpty();
            compressedUrl.CreatedAt.Should().Be(createdAt);
            compressedUrl.ExpiresAt.Should().Be(expiresAt);
            compressedUrl.TotalClicks.Should().Be(clickCount);
            compressedUrl.CompressedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }
        
        [Fact]
        public void Create_WithNullShortUrl_ThrowsArgumentNullException()
        {
            // Arrange
            ShortUrl? shortUrl = null;
            
            // Act & Assert
            Action act = () => CompressedShortUrl.Create(shortUrl!);
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("shortUrl");
        }
        
        [Fact]
        public void GetOriginalUrl_ReturnsDecompressedUrl()
        {
            // Arrange
            var originalUrl = "https://example.com/very/long/path/to/resource";
            var shortUrl = TestDataFactory.CreateValidShortUrl(originalUrl: originalUrl);
            var compressedUrl = CompressedShortUrl.Create(shortUrl);
            
            // Act
            var decompressedUrl = compressedUrl.GetOriginalUrl();
            
            // Assert
            decompressedUrl.Should().Be(originalUrl);
        }
        
        [Fact]
        public void Create_WithLongUrl_CompressesEffectively()
        {
            // Arrange
            var longUrl = new string('a', 1000); // 1000 character URL
            var shortUrl = TestDataFactory.CreateValidShortUrl(originalUrl: longUrl);
            
            // Act
            var compressedUrl = CompressedShortUrl.Create(shortUrl);
            
            // Assert
            compressedUrl.CompressedData.Length.Should().BeLessThan(longUrl.Length);
        }
    }
} 