using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Microsoft.Extensions.Logging;
using URLShortener.API.Controllers;
using MediatR;

namespace URLShortener.Tests.API
{
    public class MockControllerTests
    {
        [Fact]
        public void Controller_WithMockedDependencies_CanBeCreated()
        {
            // Arrange
            var mockMediator = new Mock<IMediator>();
            var mockLogger = new Mock<ILogger<ShortUrlController>>();
            
            // Act
            var controller = new ShortUrlController(mockMediator.Object, mockLogger.Object);
            
            // Assert
            controller.Should().NotBeNull();
        }
    }
} 