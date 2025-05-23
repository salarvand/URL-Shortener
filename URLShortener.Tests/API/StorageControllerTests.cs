using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using URLShortener.API.Controllers;
using URLShortener.Application.Interfaces;

namespace URLShortener.Tests.API
{
    public class StorageControllerTests
    {
        private readonly Mock<IStorageOptimizer> _mockOptimizer;
        private readonly Mock<ILogger<StorageController>> _mockLogger;
        private readonly StorageController _controller;
        
        public StorageControllerTests()
        {
            _mockOptimizer = new Mock<IStorageOptimizer>();
            _mockLogger = new Mock<ILogger<StorageController>>();
            _controller = new StorageController(_mockOptimizer.Object, _mockLogger.Object);
        }
        
        [Fact]
        public async Task GetStatistics_ReturnsOkWithStats()
        {
            // Arrange
            var stats = new StorageStatistics
            {
                TotalUrls = 100,
                ActiveUrls = 80,
                ExpiredUrls = 20,
                TotalClickStatistics = 500,
                UrlStorageBytes = 20000,
                ClickStatisticsStorageBytes = 50000
            };
            
            _mockOptimizer.Setup(o => o.GetStorageStatisticsAsync()).ReturnsAsync(stats);
            
            // Act
            var result = await _controller.GetStatistics();
            
            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);
            
            var returnValue = okResult.Value as StorageStatistics;
            returnValue.Should().NotBeNull();
            returnValue!.TotalUrls.Should().Be(100);
            returnValue.ActiveUrls.Should().Be(80);
            returnValue.TotalStorageBytes.Should().Be(70000);
        }
        
        [Fact]
        public async Task GetStatistics_WhenExceptionOccurs_ReturnsInternalServerError()
        {
            // Arrange
            _mockOptimizer.Setup(o => o.GetStorageStatisticsAsync())
                .ThrowsAsync(new Exception("Database error"));
            
            // Act
            var result = await _controller.GetStatistics();
            
            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult!.StatusCode.Should().Be(500);
            
            var errorObject = statusCodeResult.Value as object;
            errorObject.Should().NotBeNull();
        }
        
        [Fact]
        public async Task PurgeExpiredUrls_ReturnsOkWithCount()
        {
            // Arrange
            int purgedCount = 15;
            
            _mockOptimizer.Setup(o => o.PurgeExpiredUrlsAsync()).ReturnsAsync(purgedCount);
            
            // Act
            var result = await _controller.PurgeExpiredUrls();
            
            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);
            
            // Verify the result contains an object with a property named purgedCount
            okResult.Value.Should().NotBeNull();
            var dict = okResult.Value.GetType().GetProperties()
                .ToDictionary(p => p.Name, p => p.GetValue(okResult.Value));
            dict.Should().ContainKey("purgedCount");
            dict["purgedCount"].Should().Be(purgedCount);
        }
        
        [Fact]
        public async Task PurgeExpiredUrls_WhenExceptionOccurs_ReturnsInternalServerError()
        {
            // Arrange
            _mockOptimizer.Setup(o => o.PurgeExpiredUrlsAsync())
                .ThrowsAsync(new Exception("Database error"));
            
            // Act
            var result = await _controller.PurgeExpiredUrls();
            
            // Assert
            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult!.StatusCode.Should().Be(500);
        }
        
        [Fact]
        public async Task AggregateClickStatistics_WithDefaultDays_ReturnsOkWithCount()
        {
            // Arrange
            int aggregatedCount = 200;
            
            _mockOptimizer.Setup(o => o.AggregateOldClickStatisticsAsync(It.IsAny<TimeSpan>()))
                .ReturnsAsync(aggregatedCount);
            
            // Act
            var result = await _controller.AggregateClickStatistics(); // Default 30 days
            
            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);
            
            // Verify the result contains an object with a property named aggregatedCount
            okResult.Value.Should().NotBeNull();
            var dict = okResult.Value.GetType().GetProperties()
                .ToDictionary(p => p.Name, p => p.GetValue(okResult.Value));
            dict.Should().ContainKey("aggregatedCount");
            dict["aggregatedCount"].Should().Be(aggregatedCount);
            
            // Verify the correct time span was used
            _mockOptimizer.Verify(o => o.AggregateOldClickStatisticsAsync(
                It.Is<TimeSpan>(ts => ts.TotalDays == 30)), Times.Once);
        }
        
        [Fact]
        public async Task AggregateClickStatistics_WithCustomDays_ReturnsOkWithCount()
        {
            // Arrange
            int aggregatedCount = 150;
            int customDays = 60;
            
            _mockOptimizer.Setup(o => o.AggregateOldClickStatisticsAsync(It.IsAny<TimeSpan>()))
                .ReturnsAsync(aggregatedCount);
            
            // Act
            var result = await _controller.AggregateClickStatistics(customDays);
            
            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);
            
            // Verify the result contains an object with a property named aggregatedCount
            okResult.Value.Should().NotBeNull();
            var dict = okResult.Value.GetType().GetProperties()
                .ToDictionary(p => p.Name, p => p.GetValue(okResult.Value));
            dict.Should().ContainKey("aggregatedCount");
            dict["aggregatedCount"].Should().Be(aggregatedCount);
            
            // Verify the correct time span was used
            _mockOptimizer.Verify(o => o.AggregateOldClickStatisticsAsync(
                It.Is<TimeSpan>(ts => ts.TotalDays == 60)), Times.Once);
        }
        
        [Fact]
        public async Task CompressOldUrlData_WithDefaultDays_ReturnsOkWithCount()
        {
            // Arrange
            int compressedCount = 25;
            
            _mockOptimizer.Setup(o => o.CompressOldUrlDataAsync(It.IsAny<TimeSpan>()))
                .ReturnsAsync(compressedCount);
            
            // Act
            var result = await _controller.CompressOldUrlData(); // Default 90 days
            
            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);
            
            // Verify the result contains an object with a property named compressedCount
            okResult.Value.Should().NotBeNull();
            var dict = okResult.Value.GetType().GetProperties()
                .ToDictionary(p => p.Name, p => p.GetValue(okResult.Value));
            dict.Should().ContainKey("compressedCount");
            dict["compressedCount"].Should().Be(compressedCount);
            
            // Verify the correct time span was used
            _mockOptimizer.Verify(o => o.CompressOldUrlDataAsync(
                It.Is<TimeSpan>(ts => ts.TotalDays == 90)), Times.Once);
        }
        
        [Fact]
        public async Task CompressOldUrlData_WithCustomDays_ReturnsOkWithCount()
        {
            // Arrange
            int compressedCount = 15;
            int customDays = 120;
            
            _mockOptimizer.Setup(o => o.CompressOldUrlDataAsync(It.IsAny<TimeSpan>()))
                .ReturnsAsync(compressedCount);
            
            // Act
            var result = await _controller.CompressOldUrlData(customDays);
            
            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);
            
            // Verify the result contains an object with a property named compressedCount
            okResult.Value.Should().NotBeNull();
            var dict = okResult.Value.GetType().GetProperties()
                .ToDictionary(p => p.Name, p => p.GetValue(okResult.Value));
            dict.Should().ContainKey("compressedCount");
            dict["compressedCount"].Should().Be(compressedCount);
            
            // Verify the correct time span was used
            _mockOptimizer.Verify(o => o.CompressOldUrlDataAsync(
                It.Is<TimeSpan>(ts => ts.TotalDays == 120)), Times.Once);
        }
    }
} 