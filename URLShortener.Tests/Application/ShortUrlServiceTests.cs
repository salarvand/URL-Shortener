using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using URLShortener.Application.Interfaces;
using URLShortener.Application.Models;
using URLShortener.Application.Services;
using URLShortener.Domain;
using URLShortener.Domain.Entities;
using URLShortener.Tests.Common;
using URLShortener.Tests.Interfaces;
using URLShortener.Tests.Models;

namespace URLShortener.Tests.Application
{
    public class ShortUrlServiceTests
    {
        private readonly Mock<IShortUrlRepository> _mockRepository;
        private readonly Mock<IShortCodeGenerator> _mockGenerator;
        private readonly Mock<IValidationService> _mockValidationService;
        private readonly ShortUrlService _service;

        public ShortUrlServiceTests()
        {
            _mockRepository = new Mock<IShortUrlRepository>();
            _mockGenerator = new Mock<IShortCodeGenerator>();
            _mockValidationService = new Mock<IValidationService>();
            
            _service = new ShortUrlService(
                _mockRepository.Object,
                _mockGenerator.Object,
                _mockValidationService.Object
            );
        }

        [Fact]
        public async Task CreateShortUrlAsync_WithValidUrl_CreatesAndReturnsShortUrl()
        {
            // Arrange
            var originalUrl = "https://example.com";
            var generatedCode = "abc123";
            var dto = new CreateShortUrlDto 
            { 
                OriginalUrl = originalUrl
            };
            
            _mockValidationService.Setup(v => v.ValidateAndThrowAsync(dto)).Returns(Task.CompletedTask);
            _mockRepository.Setup(r => r.ShortCodeExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            _mockGenerator.Setup(g => g.GenerateShortCode(It.IsAny<int>()))
                .Returns(generatedCode);
            
            // Create a ShortUrl instance to be returned by AddAsync
            var shortUrl = ShortUrl.Create(originalUrl, generatedCode);
            _mockRepository.Setup(r => r.AddAsync(It.IsAny<ShortUrl>())).ReturnsAsync(shortUrl);
            
            // Act
            var result = await _service.CreateShortUrlAsync(dto);
            
            // Assert
            result.Should().NotBeNull();
            result.OriginalUrl.Should().Be(originalUrl);
            result.ShortCode.Should().Be(generatedCode);
            
            // Verify generator was called
            _mockGenerator.Verify(g => g.GenerateShortCode(It.IsAny<int>()), Times.Once);
            
            // Verify repository was called with a ShortUrl
            _mockRepository.Verify(r => r.AddAsync(It.Is<ShortUrl>(
                s => s.OriginalUrl == originalUrl && 
                     s.ShortCode == generatedCode)), 
                Times.Once);
        }
        
        [Fact]
        public async Task CreateShortUrlAsync_WithCustomShortCode_UsesProvidedCode()
        {
            // Arrange
            var originalUrl = "https://example.com";
            var customCode = "custom123";
            var dto = new CreateShortUrlDto 
            { 
                OriginalUrl = originalUrl,
                CustomShortCode = customCode
            };
            
            _mockValidationService.Setup(v => v.ValidateAndThrowAsync(dto)).Returns(Task.CompletedTask);
            _mockRepository.Setup(r => r.ShortCodeExistsAsync(customCode)).ReturnsAsync(false);
            
            // Create a ShortUrl instance to be returned by AddAsync
            var shortUrl = ShortUrl.Create(originalUrl, customCode);
            _mockRepository.Setup(r => r.AddAsync(It.IsAny<ShortUrl>())).ReturnsAsync(shortUrl);
            
            // Act
            var result = await _service.CreateShortUrlAsync(dto);
            
            // Assert
            result.Should().NotBeNull();
            result.OriginalUrl.Should().Be(originalUrl);
            result.ShortCode.Should().Be(customCode);
            
            // Verify generator was not called
            _mockGenerator.Verify(g => g.GenerateShortCode(It.IsAny<int>()), Times.Never);
            
            // Verify repository was called with a ShortUrl
            _mockRepository.Verify(r => r.AddAsync(It.Is<ShortUrl>(
                s => s.OriginalUrl == originalUrl && 
                     s.ShortCode == customCode)), 
                Times.Once);
        }
        
        [Fact]
        public async Task CreateShortUrlAsync_WithExpiryDate_SetsExpiryDate()
        {
            // Arrange
            var originalUrl = "https://example.com";
            var generatedCode = "abc123";
            var expiryDate = DateTime.UtcNow.AddDays(7);
            var dto = new CreateShortUrlDto 
            { 
                OriginalUrl = originalUrl,
                ExpiresAt = expiryDate
            };
            
            _mockValidationService.Setup(v => v.ValidateAndThrowAsync(dto)).Returns(Task.CompletedTask);
            _mockRepository.Setup(r => r.ShortCodeExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            _mockGenerator.Setup(g => g.GenerateShortCode(It.IsAny<int>()))
                .Returns(generatedCode);
            
            // Create a ShortUrl instance with expiry date to be returned by AddAsync
            var shortUrl = ShortUrl.Create(originalUrl, generatedCode, expiryDate);
            _mockRepository.Setup(r => r.AddAsync(It.IsAny<ShortUrl>())).ReturnsAsync(shortUrl);
            
            // Act
            var result = await _service.CreateShortUrlAsync(dto);
            
            // Assert
            result.Should().NotBeNull();
            result.OriginalUrl.Should().Be(originalUrl);
            result.ShortCode.Should().Be(generatedCode);
            result.ExpiresAt.Should().Be(expiryDate);
            
            // Verify repository was called with a ShortUrl containing expiry date
            _mockRepository.Verify(r => r.AddAsync(It.Is<ShortUrl>(
                s => s.OriginalUrl == originalUrl && 
                     s.ShortCode == generatedCode &&
                     s.ExpiresAt == expiryDate)), 
                Times.Once);
        }
        
        [Fact]
        public async Task CreateShortUrlAsync_WithInvalidDto_ThrowsException()
        {
            // Arrange
            var invalidUrl = "not-a-valid-url";
            var dto = new CreateShortUrlDto { OriginalUrl = invalidUrl };
            
            _mockValidationService.Setup(v => v.ValidateAndThrowAsync(dto)).ThrowsAsync(new Exception("Validation failed"));
            
            // Act & Assert
            await _service.Invoking(s => s.CreateShortUrlAsync(dto))
                .Should().ThrowAsync<Exception>()
                .WithMessage("Validation failed");
        }
        
        [Fact]
        public async Task CreateShortUrlAsync_WithDuplicateCustomShortCode_ThrowsInvalidOperationException()
        {
            // Arrange
            var originalUrl = "https://example.com";
            var existingShortCode = "existing";
            var dto = new CreateShortUrlDto 
            { 
                OriginalUrl = originalUrl,
                CustomShortCode = existingShortCode
            };
            
            _mockValidationService.Setup(v => v.ValidateAndThrowAsync(dto)).Returns(Task.CompletedTask);
            _mockRepository.Setup(r => r.ShortCodeExistsAsync(existingShortCode)).ReturnsAsync(true);
            
            // Act & Assert
            await _service.Invoking(s => s.CreateShortUrlAsync(dto))
                .Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"Short code '{existingShortCode}' is already in use");
        }
        
        [Fact]
        public async Task GetShortUrlDetailsByIdAsync_WithExistingId_ReturnsShortUrlDto()
        {
            // Arrange
            var id = Guid.NewGuid();
            var originalUrl = "https://example.com";
            var shortCode = "abc123";
            var createdAt = DateTime.UtcNow.AddDays(-1);
            
            var shortUrl = ShortUrl.Reconstitute(
                id,
                originalUrl,
                shortCode,
                createdAt,
                0,  // click count
                true // isActive
            );
            
            _mockRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(shortUrl);
            
            // Act
            var result = await _service.GetShortUrlDetailsByIdAsync(id);
            
            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(id);
            result.OriginalUrl.Should().Be(originalUrl);
            result.ShortCode.Should().Be(shortCode);
        }
        
        [Fact]
        public async Task GetShortUrlDetailsByIdAsync_WithNonExistingId_ReturnsNull()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid();
            
            _mockRepository.Setup(r => r.GetByIdAsync(nonExistingId)).ReturnsAsync((ShortUrl)null);
            
            // Act
            var result = await _service.GetShortUrlDetailsByIdAsync(nonExistingId);
            
            // Assert
            result.Should().BeNull();
        }
        
        [Fact]
        public async Task GetShortUrlByCodeAsync_WithExistingCode_ReturnsShortUrlDto()
        {
            // Arrange
            var shortCode = "abc123";
            var originalUrl = "https://example.com";
            var createdAt = DateTime.UtcNow.AddDays(-1);
            
            var shortUrl = ShortUrl.Reconstitute(
                Guid.NewGuid(),
                originalUrl,
                shortCode,
                createdAt,
                0,
                true
            );
            
            _mockRepository.Setup(r => r.GetByShortCodeAsync(shortCode)).ReturnsAsync(shortUrl);
            
            // Act
            var result = await _service.GetShortUrlByCodeAsync(shortCode);
            
            // Assert
            result.Should().NotBeNull();
            result.ShortCode.Should().Be(shortCode);
        }
        
        [Fact]
        public async Task GetShortUrlByCodeAsync_WithNonExistingCode_ReturnsNull()
        {
            // Arrange
            var nonExistingCode = "nonexistent";
            
            _mockRepository.Setup(r => r.GetByShortCodeAsync(nonExistingCode)).ReturnsAsync((ShortUrl)null);
            
            // Act
            var result = await _service.GetShortUrlByCodeAsync(nonExistingCode);
            
            // Assert
            result.Should().BeNull();
        }
        
        [Fact]
        public async Task GetAllShortUrlsAsync_ReturnsAllShortUrls()
        {
            // Arrange
            var shortUrls = new List<ShortUrl>
            {
                ShortUrl.Create("https://example1.com", "code1"),
                ShortUrl.Create("https://example2.com", "code2"),
                ShortUrl.Create("https://example3.com", "code3")
            };
            
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(shortUrls);
            
            // Act
            var result = await _service.GetAllShortUrlsAsync();
            
            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
        }
        
        [Fact]
        public async Task RedirectAndTrackAsync_WithExistingCode_ReturnsOriginalUrl()
        {
            // Arrange
            var shortCode = "abc123";
            var originalUrl = "https://example.com";
            
            var shortUrl = ShortUrl.Create(originalUrl, shortCode);
            
            _mockRepository.Setup(r => r.GetByShortCodeAsync(shortCode)).ReturnsAsync(shortUrl);
            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<ShortUrl>())).Returns(Task.CompletedTask);
            
            // Act
            var result = await _service.RedirectAndTrackAsync(shortCode);
            
            // Assert
            result.Should().Be(originalUrl);
            _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<ShortUrl>()), Times.Once);
        }
        
        [Fact]
        public async Task RedirectAndTrackAsync_WithNonExistingCode_ReturnsNull()
        {
            // Arrange
            var nonExistingCode = "nonexistent";
            
            _mockRepository.Setup(r => r.GetByShortCodeAsync(nonExistingCode)).ReturnsAsync((ShortUrl)null);
            
            // Act
            var result = await _service.RedirectAndTrackAsync(nonExistingCode);
            
            // Assert
            result.Should().BeNull();
            _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<ShortUrl>()), Times.Never);
        }
    }
} 