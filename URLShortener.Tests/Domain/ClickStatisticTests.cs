using System;
using URLShortener.Domain.Entities;

namespace URLShortener.Tests.Domain
{
    public class ClickStatisticTests
    {
        [Fact]
        public void Create_WithValidParameters_ReturnsClickStatistic()
        {
            // Arrange
            var shortUrlId = Guid.NewGuid();
            var userAgent = "Mozilla/5.0";
            var ipAddress = "192.168.1.1";
            var refererUrl = "https://google.com";
            
            // Act
            var clickStat = ClickStatistic.Create(shortUrlId, userAgent, ipAddress, refererUrl);
            
            // Assert
            clickStat.Should().NotBeNull();
            clickStat.Id.Should().NotBeEmpty();
            clickStat.ShortUrlId.Should().Be(shortUrlId);
            clickStat.ClickedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            clickStat.UserAgent.Should().Be(userAgent);
            clickStat.IpAddress.Should().Be(ipAddress);
            clickStat.RefererUrl.Should().Be(refererUrl);
        }
        
        [Fact]
        public void Create_WithMinimalParameters_ReturnsClickStatistic()
        {
            // Arrange
            var shortUrlId = Guid.NewGuid();
            
            // Act
            var clickStat = ClickStatistic.Create(shortUrlId);
            
            // Assert
            clickStat.Should().NotBeNull();
            clickStat.Id.Should().NotBeEmpty();
            clickStat.ShortUrlId.Should().Be(shortUrlId);
            clickStat.ClickedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            clickStat.UserAgent.Should().BeNull();
            clickStat.IpAddress.Should().BeNull();
            clickStat.RefererUrl.Should().BeNull();
        }
        
        [Fact]
        public void Create_WithEmptyShortUrlId_ThrowsArgumentException()
        {
            // Arrange
            var shortUrlId = Guid.Empty;
            
            // Act & Assert
            Action act = () => ClickStatistic.Create(shortUrlId);
            act.Should().Throw<ArgumentException>()
                .WithMessage("ShortUrl ID cannot be empty*");
        }
    }
} 