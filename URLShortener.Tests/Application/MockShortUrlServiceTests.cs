using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using URLShortener.Application.Interfaces;
using URLShortener.Application.Models;
using URLShortener.Tests.Models;

namespace URLShortener.Tests.Application
{
    public class MockShortUrlServiceTests
    {
        [Fact]
        public async Task GetShortUrlByCodeAsync_ReturnsShortUrlDto_WhenShortCodeExists()
        {
            // Arrange
            var mockService = new Mock<IShortUrlService>();
            var shortCode = "abc123";
            var expectedDto = new ShortUrlDto
            {
                Id = Guid.NewGuid(),
                ShortCode = shortCode,
                OriginalUrl = "https://example.com",
                ShortUrl = $"https://short.url/{shortCode}",
                CreatedAt = DateTime.UtcNow,
                ClickCount = 0
            };
            
            mockService.Setup(s => s.GetShortUrlByCodeAsync(shortCode))
                .ReturnsAsync(expectedDto);
                
            // Act
            var result = await mockService.Object.GetShortUrlByCodeAsync(shortCode);
            
            // Assert
            result.Should().NotBeNull();
            result.ShortCode.Should().Be(shortCode);
            result.OriginalUrl.Should().Be("https://example.com");
        }
        
        [Fact]
        public async Task GetShortUrlByCodeAsync_ReturnsNull_WhenShortCodeDoesNotExist()
        {
            // Arrange
            var mockService = new Mock<IShortUrlService>();
            var nonExistentShortCode = "nonexistent";
            
            mockService.Setup(s => s.GetShortUrlByCodeAsync(nonExistentShortCode))
                .ReturnsAsync((ShortUrlDto)null);
                
            // Act
            var result = await mockService.Object.GetShortUrlByCodeAsync(nonExistentShortCode);
            
            // Assert
            result.Should().BeNull();
        }
        
        [Fact]
        public async Task RedirectAndTrackAsync_ReturnsOriginalUrl_AndTracksClick()
        {
            // Arrange
            var mockService = new Mock<IShortUrlService>();
            var shortCode = "abc123";
            var originalUrl = "https://example.com";
            
            mockService.Setup(s => s.RedirectAndTrackAsync(shortCode))
                .ReturnsAsync(originalUrl);
                
            // Act
            var result = await mockService.Object.RedirectAndTrackAsync(shortCode);
            
            // Assert
            result.Should().NotBeNull();
            result.Should().Be(originalUrl);
            
            // Verify the method was called once
            mockService.Verify(s => s.RedirectAndTrackAsync(shortCode), Times.Once);
        }
    }
} 