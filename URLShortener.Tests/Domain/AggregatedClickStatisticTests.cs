using System;
using URLShortener.Domain.Entities;

namespace URLShortener.Tests.Domain
{
    public class AggregatedClickStatisticTests
    {
        [Fact]
        public void Create_WithValidParameters_ReturnsAggregatedClickStatistic()
        {
            // Arrange
            var shortUrlId = Guid.NewGuid();
            var periodStart = DateTime.UtcNow.AddDays(-30);
            var periodEnd = DateTime.UtcNow;
            var clickCount = 100;
            var userAgentSummary = "{\"Chrome\":60,\"Firefox\":30,\"Safari\":10}";
            var geographicSummary = "{\"US\":50,\"UK\":30,\"CA\":20}";
            var refererSummary = "{\"Google\":70,\"Bing\":20,\"Direct\":10}";
            
            // Act
            var aggregatedStat = AggregatedClickStatistic.Create(
                shortUrlId, 
                periodStart, 
                periodEnd, 
                clickCount,
                userAgentSummary,
                geographicSummary,
                refererSummary);
            
            // Assert
            aggregatedStat.Should().NotBeNull();
            aggregatedStat.Id.Should().NotBeEmpty();
            aggregatedStat.ShortUrlId.Should().Be(shortUrlId);
            aggregatedStat.PeriodStart.Should().Be(periodStart);
            aggregatedStat.PeriodEnd.Should().Be(periodEnd);
            aggregatedStat.ClickCount.Should().Be(clickCount);
            aggregatedStat.UserAgentSummary.Should().Be(userAgentSummary);
            aggregatedStat.GeographicSummary.Should().Be(geographicSummary);
            aggregatedStat.RefererSummary.Should().Be(refererSummary);
            aggregatedStat.IsCompressed.Should().BeFalse();
        }
        
        [Fact]
        public void Create_WithPeriodEndBeforePeriodStart_ThrowsArgumentException()
        {
            // Arrange
            var shortUrlId = Guid.NewGuid();
            var periodStart = DateTime.UtcNow;
            var periodEnd = DateTime.UtcNow.AddDays(-1); // End before start
            var clickCount = 100;
            
            // Act & Assert
            Action act = () => AggregatedClickStatistic.Create(shortUrlId, periodStart, periodEnd, clickCount);
            act.Should().Throw<ArgumentException>()
                .WithMessage("Period end must be after period start");
        }
        
        [Fact]
        public void Create_WithNegativeClickCount_ThrowsArgumentException()
        {
            // Arrange
            var shortUrlId = Guid.NewGuid();
            var periodStart = DateTime.UtcNow.AddDays(-30);
            var periodEnd = DateTime.UtcNow;
            var clickCount = -10; // Negative click count
            
            // Act & Assert
            Action act = () => AggregatedClickStatistic.Create(shortUrlId, periodStart, periodEnd, clickCount);
            act.Should().Throw<ArgumentException>()
                .WithMessage("Click count cannot be negative");
        }
        
        [Fact]
        public void SetCompressed_SetsIsCompressedFlag()
        {
            // Arrange
            var shortUrlId = Guid.NewGuid();
            var periodStart = DateTime.UtcNow.AddDays(-30);
            var periodEnd = DateTime.UtcNow;
            var clickCount = 100;
            var aggregatedStat = AggregatedClickStatistic.Create(shortUrlId, periodStart, periodEnd, clickCount);
            
            // Act
            aggregatedStat.SetCompressed(true);
            
            // Assert
            aggregatedStat.IsCompressed.Should().BeTrue();
        }
        
        [Fact]
        public void AddClicks_IncreasesClickCount()
        {
            // Arrange
            var shortUrlId = Guid.NewGuid();
            var periodStart = DateTime.UtcNow.AddDays(-30);
            var periodEnd = DateTime.UtcNow;
            var initialClickCount = 100;
            var aggregatedStat = AggregatedClickStatistic.Create(shortUrlId, periodStart, periodEnd, initialClickCount);
            var additionalClicks = 50;
            
            // Act
            aggregatedStat.AddClicks(additionalClicks);
            
            // Assert
            aggregatedStat.ClickCount.Should().Be(initialClickCount + additionalClicks);
        }
        
        [Fact]
        public void AddClicks_WithNegativeValue_ThrowsArgumentException()
        {
            // Arrange
            var shortUrlId = Guid.NewGuid();
            var periodStart = DateTime.UtcNow.AddDays(-30);
            var periodEnd = DateTime.UtcNow;
            var clickCount = 100;
            var aggregatedStat = AggregatedClickStatistic.Create(shortUrlId, periodStart, periodEnd, clickCount);
            var negativeClicks = -10;
            
            // Act & Assert
            Action act = () => aggregatedStat.AddClicks(negativeClicks);
            act.Should().Throw<ArgumentException>()
                .WithMessage("Additional clicks cannot be negative");
        }
    }
} 