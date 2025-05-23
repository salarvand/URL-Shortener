using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using URLShortener.Domain;
using URLShortener.Domain.Entities;
using URLShortener.Infrastructure.Data;
using URLShortener.Infrastructure.Repositories;
using URLShortener.Tests.Common;

namespace URLShortener.Tests.Infrastructure
{
    // Test implementation of ShortUrlRepository with additional methods
    public class TestShortUrlRepository : ShortUrlRepository
    {
        private readonly AppDbContext _dbContext;
        
        public TestShortUrlRepository(AppDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
        
        public async Task DeleteAsync(Guid id)
        {
            var shortUrl = await _dbContext.ShortUrls.FindAsync(id);
            if (shortUrl != null)
            {
                _dbContext.ShortUrls.Remove(shortUrl);
                await _dbContext.SaveChangesAsync();
            }
        }
        
        public async Task AddClickStatisticAsync(ClickStatistic clickStatistic)
        {
            await _dbContext.ClickStatistics.AddAsync(clickStatistic);
            await _dbContext.SaveChangesAsync();
        }
        
        public async Task<IEnumerable<ClickStatistic>> GetClickStatisticsAsync(Guid shortUrlId)
        {
            return await _dbContext.ClickStatistics
                .Where(c => c.ShortUrlId == shortUrlId)
                .ToListAsync();
        }
    }

    public class ShortUrlRepositoryTests : TestBase
    {
        [Fact]
        public async Task GetByIdAsync_WithExistingId_ReturnsShortUrl()
        {
            // Arrange
            var shortUrl = TestDataFactory.CreateValidShortUrl();
            AddEntity(shortUrl);
            
            var repository = new ShortUrlRepository(GetDbContext());
            
            // Act
            var result = await repository.GetByIdAsync(shortUrl.Id);
            
            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(shortUrl.Id);
            result.OriginalUrl.Should().Be(shortUrl.OriginalUrl);
            result.ShortCode.Should().Be(shortUrl.ShortCode);
        }
        
        [Fact]
        public async Task GetByIdAsync_WithNonExistingId_ReturnsNull()
        {
            // Arrange
            var repository = new ShortUrlRepository(GetDbContext());
            
            // Act
            var result = await repository.GetByIdAsync(Guid.NewGuid());
            
            // Assert
            result.Should().BeNull();
        }
        
        [Fact]
        public async Task GetByShortCodeAsync_WithExistingShortCode_ReturnsShortUrl()
        {
            // Arrange
            var shortUrl = TestDataFactory.CreateValidShortUrl(shortCode: "unique123");
            AddEntity(shortUrl);
            
            var repository = new ShortUrlRepository(GetDbContext());
            
            // Act
            var result = await repository.GetByShortCodeAsync("unique123");
            
            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(shortUrl.Id);
            result.ShortCode.Should().Be("unique123");
        }
        
        [Fact]
        public async Task GetByShortCodeAsync_WithNonExistingShortCode_ReturnsNull()
        {
            // Arrange
            var repository = new ShortUrlRepository(GetDbContext());
            
            // Act
            var result = await repository.GetByShortCodeAsync("nonexistent");
            
            // Assert
            result.Should().BeNull();
        }
        
        [Fact]
        public async Task AddAsync_AddsShortUrlToDatabase()
        {
            // Arrange
            var shortUrl = TestDataFactory.CreateValidShortUrl();
            var repository = new ShortUrlRepository(GetDbContext());
            
            // Act
            await repository.AddAsync(shortUrl);
            
            // Assert
            var dbContext = GetDbContext();
            var savedShortUrl = await dbContext.ShortUrls.FindAsync(shortUrl.Id);
            savedShortUrl.Should().NotBeNull();
            savedShortUrl!.ShortCode.Should().Be(shortUrl.ShortCode);
        }
        
        [Fact]
        public async Task UpdateAsync_UpdatesExistingShortUrl()
        {
            // Arrange
            var shortUrl = TestDataFactory.CreateValidShortUrl();
            AddEntity(shortUrl);
            
            var repository = new ShortUrlRepository(GetDbContext());
            
            // Modify the short URL
            shortUrl.Deactivate();
            shortUrl.SetExpiryDate(DateTime.UtcNow.AddDays(10));
            
            // Act
            await repository.UpdateAsync(shortUrl);
            
            // Assert
            var dbContext = GetDbContext();
            var updatedShortUrl = await dbContext.ShortUrls.FindAsync(shortUrl.Id);
            updatedShortUrl.Should().NotBeNull();
            updatedShortUrl!.IsActive.Should().BeFalse();
            updatedShortUrl.ExpiresAt.Should().NotBeNull();
        }
        
        [Fact]
        public async Task DeleteAsync_RemovesShortUrlFromDatabase()
        {
            // Arrange
            var shortUrl = TestDataFactory.CreateValidShortUrl();
            AddEntity(shortUrl);
            
            // Create a repository implementation that implements DeleteAsync
            var repository = new TestShortUrlRepository(GetDbContext());
            
            // Act
            await repository.DeleteAsync(shortUrl.Id);
            
            // Assert
            var dbContext = GetDbContext();
            var deletedShortUrl = await dbContext.ShortUrls.FindAsync(shortUrl.Id);
            deletedShortUrl.Should().BeNull();
        }
        
        [Fact]
        public async Task GetAllAsync_ReturnsAllShortUrls()
        {
            // Arrange
            var shortUrls = TestDataFactory.CreateMultipleShortUrls(5);
            AddEntities(shortUrls);
            
            var repository = new ShortUrlRepository(GetDbContext());
            
            // Act
            var result = await repository.GetAllAsync();
            
            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(5);
        }
        
        [Fact]
        public async Task AddClickStatisticAsync_AddsClickStatToDatabase()
        {
            // Arrange
            var shortUrl = TestDataFactory.CreateValidShortUrl();
            AddEntity(shortUrl);
            
            var clickStat = ClickStatistic.Create(
                shortUrl.Id,
                "Chrome",
                "192.168.1.1",
                "https://referrer.com"
            );
            
            // Create a repository implementation that implements AddClickStatisticAsync
            var repository = new TestShortUrlRepository(GetDbContext());
            
            // Act
            await repository.AddClickStatisticAsync(clickStat);
            
            // Assert
            var dbContext = GetDbContext();
            var savedClickStat = await dbContext.ClickStatistics
                .FirstOrDefaultAsync(c => c.Id == clickStat.Id);
                
            savedClickStat.Should().NotBeNull();
            savedClickStat!.ShortUrlId.Should().Be(shortUrl.Id);
            savedClickStat.UserAgent.Should().Be("Chrome");
            savedClickStat.IpAddress.Should().Be("192.168.1.1");
            savedClickStat.RefererUrl.Should().Be("https://referrer.com");
        }
        
        [Fact]
        public async Task GetClickStatisticsAsync_ReturnsClickStatsForShortUrl()
        {
            // Arrange
            var shortUrl = TestDataFactory.CreateValidShortUrl();
            AddEntity(shortUrl);
            
            // Add click statistics for this URL
            var clickStats = TestDataFactory.CreateMultipleClickStatistics(5, shortUrl.Id);
            AddEntities(clickStats);
            
            // Add some click statistics for a different URL
            var otherShortUrl = TestDataFactory.CreateValidShortUrl();
            AddEntity(otherShortUrl);
            var otherClickStats = TestDataFactory.CreateMultipleClickStatistics(3, otherShortUrl.Id);
            AddEntities(otherClickStats);
            
            // Create a repository implementation that implements GetClickStatisticsAsync
            var repository = new TestShortUrlRepository(GetDbContext());
            
            // Act
            var result = await repository.GetClickStatisticsAsync(shortUrl.Id);
            
            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(5);
            result.Should().AllSatisfy(c => c.ShortUrlId.Should().Be(shortUrl.Id));
        }
    }
} 