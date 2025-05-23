using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using MediatR;
using URLShortener.API.Controllers;
using URLShortener.Application.Features.ShortUrls;
using URLShortener.Application.Models;
using URLShortener.Tests.Common;
using URLShortener.Tests.Models;

namespace URLShortener.Tests.API
{
    /// <summary>
    /// All tests in this class are skipped due to interface incompatibility with MediatR.
    /// The controller now uses MediatR instead of IShortUrlService directly.
    /// </summary>
    public class ShortUrlControllerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<ShortUrlController>> _mockLogger;
        private readonly ShortUrlController _controller;
        
        public ShortUrlControllerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<ShortUrlController>>();
            
            _controller = new ShortUrlController(_mockMediator.Object, _mockLogger.Object);
            
            // Set up HttpContext for controller
            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }
        
        [Fact]
        public async Task Create_WithValidDto_ReturnsCreatedResult()
        {
            // Arrange
            var command = new CreateShortUrl.Command
            {
                OriginalUrl = "https://example.com",
                CustomShortCode = "test123",
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };
            
            var shortUrlDto = new ShortUrlDto
            {
                Id = Guid.NewGuid(),
                OriginalUrl = "https://example.com",
                ShortCode = "test123",
                ShortUrl = "/s/test123",
                CreatedAt = DateTime.UtcNow,
                ClickCount = 0,
                IsActive = true
            };
            
            _mockMediator.Setup(m => m.Send(It.IsAny<CreateShortUrl.Command>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(shortUrlDto);
            
            // Act
            var result = await _controller.Create(command);
            
            // Assert
            var createdResult = result.Result as CreatedAtActionResult;
            createdResult.Should().NotBeNull();
            createdResult!.ActionName.Should().Be(nameof(ShortUrlController.GetByCode));
            createdResult.RouteValues["code"].Should().Be("test123");
            
            var returnValue = createdResult.Value as ShortUrlDto;
            returnValue.Should().NotBeNull();
            returnValue!.OriginalUrl.Should().Be("https://example.com");
            returnValue.ShortCode.Should().Be("test123");
        }
        
        [Fact]
        public async Task GetByCode_WithExistingCode_ReturnsOk()
        {
            // Arrange
            var shortCode = "test123";
            var shortUrlDto = new ShortUrlDto
            {
                Id = Guid.NewGuid(),
                OriginalUrl = "https://example.com",
                ShortCode = shortCode,
                ShortUrl = $"/s/{shortCode}",
                CreatedAt = DateTime.UtcNow,
                ClickCount = 5,
                IsActive = true
            };
            
            _mockMediator.Setup(m => m.Send(It.Is<GetShortUrlByCode.Query>(q => q.ShortCode == shortCode), 
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(shortUrlDto);
            
            // Act
            var result = await _controller.GetByCode(shortCode);
            
            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            
            var returnValue = okResult!.Value as ShortUrlDto;
            returnValue.Should().NotBeNull();
            returnValue!.ShortCode.Should().Be(shortCode);
        }
        
        [Fact]
        public async Task GetByCode_WithNonExistingCode_ReturnsNotFound()
        {
            // Arrange
            var shortCode = "nonexistent";
            
            _mockMediator.Setup(m => m.Send(It.Is<GetShortUrlByCode.Query>(q => q.ShortCode == shortCode), 
                It.IsAny<CancellationToken>()))
                .ReturnsAsync((ShortUrlDto)null);
            
            // Act
            var result = await _controller.GetByCode(shortCode);
            
            // Assert
            result.Result.Should().BeOfType<NotFoundResult>();
        }
        
        [Fact]
        public async Task GetAll_ReturnsOkWithList()
        {
            // Arrange
            var shortUrls = new List<ShortUrlDto>
            {
                new ShortUrlDto { Id = Guid.NewGuid(), ShortCode = "code1", OriginalUrl = "https://example1.com" },
                new ShortUrlDto { Id = Guid.NewGuid(), ShortCode = "code2", OriginalUrl = "https://example2.com" }
            };
            
            _mockMediator.Setup(m => m.Send(It.IsAny<GetAllShortUrls.Query>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(shortUrls);
            
            // Act
            var result = await _controller.GetAll();
            
            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            
            var returnValue = okResult!.Value as List<ShortUrlDto>;
            returnValue.Should().NotBeNull();
            returnValue.Should().HaveCount(2);
        }
        
        [Fact]
        public async Task Redirect_WithExistingCode_ReturnsRedirectResult()
        {
            // Arrange
            var shortCode = "test123";
            var originalUrl = "https://example.com";
            
            _mockMediator.Setup(m => m.Send(It.Is<RedirectAndTrack.Command>(c => c.ShortCode == shortCode), 
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(originalUrl);
            
            // Act
            var result = await _controller.Redirect(shortCode);
            
            // Assert
            var redirectResult = result as RedirectResult;
            redirectResult.Should().NotBeNull();
            redirectResult!.Url.Should().Be(originalUrl);
        }
        
        [Fact]
        public async Task Redirect_WithNonExistingCode_ReturnsNotFound()
        {
            // Arrange
            var shortCode = "nonexistent";
            
            _mockMediator.Setup(m => m.Send(It.Is<RedirectAndTrack.Command>(c => c.ShortCode == shortCode), 
                It.IsAny<CancellationToken>()))
                .ReturnsAsync((string)null);
            
            // Act
            var result = await _controller.Redirect(shortCode);
            
            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }
    }
} 