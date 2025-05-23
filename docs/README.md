# URL Shortener Documentation

This comprehensive documentation covers all aspects of the URL Shortener project.

## Table of Contents

1. [Project Overview](#project-overview)
2. [Architecture](#architecture)
3. [Domain Model](#domain-model)
4. [Application Layer](#application-layer)
5. [Infrastructure Layer](#infrastructure-layer)
6. [API Layer](#api-layer)
7. [Testing](#testing)
8. [Deployment](#deployment)
9. [Security Considerations](#security-considerations)
10. [Performance Optimization](#performance-optimization)
11. [Monitoring and Logging](#monitoring-and-logging)
12. [Extending the System](#extending-the-system)

## Project Overview

The URL Shortener application provides a service to create shortened URLs that redirect to longer original URLs. This allows for easier sharing of links and tracking of click statistics.

### Key Features

- URL shortening with custom or auto-generated short codes
- Click tracking and statistics
- Expiration dates for shortened URLs
- REST API for all operations
- Storage optimization for old data

### Technology Stack

- **Backend**: ASP.NET Core 8
- **Database**: Microsoft SQL Server
- **Architecture**: Clean Architecture / Domain-Driven Design
- **Deployment**: Docker / Kubernetes
- **Testing**: xUnit, FluentAssertions, Moq

## Architecture

The URL Shortener follows Clean Architecture principles, with clear separation of concerns:

```
┌─────────────────────────────┐
│           API Layer         │ → Controllers, DTOs, Middleware
├─────────────────────────────┤
│       Application Layer     │ → Use Cases, Services, CQRS Commands/Queries
├─────────────────────────────┤
│         Domain Layer        │ → Entities, Value Objects, Domain Services
├─────────────────────────────┤
│    Infrastructure Layer     │ → Repositories, External Services, Database
└─────────────────────────────┘
```

### Dependency Rule

Dependencies flow inward:
- API depends on Application
- Application depends on Domain
- Domain doesn't depend on any layer
- Infrastructure depends on Domain and Application interfaces

### CQRS Pattern

The application uses Command Query Responsibility Segregation (CQRS) through MediatR:
- **Commands**: Change state (CreateShortUrl, DeleteShortUrl)
- **Queries**: Return data without changing state (GetShortUrl, GetAllShortUrls)

## Domain Model

### Core Entities

#### ShortUrl (Aggregate Root)
- Represents a shortened URL with its properties and behaviors
- Contains a collection of ClickStatistics
- Manages its own state through domain-specific methods

```csharp
public class ShortUrl : Entity, IAggregateRoot
{
    // Properties
    public string OriginalUrl { get; private set; }
    public string ShortCode { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public int ClickCount { get; private set; }
    public bool IsActive { get; private set; }
    
    // Navigation properties
    public IReadOnlyCollection<ClickStatistic> ClickStatistics { get; }
    
    // Domain methods
    public void IncrementClickCount()
    public void RecordClick(string? userAgent, string? ipAddress, string? refererUrl)
    public void Deactivate()
    public bool IsExpired()
    public void SetExpiryDate(DateTime expiryDate)
    
    // Factory methods
    public static ShortUrl Create(string originalUrl, string shortCode, DateTime? expiresAt = null)
    public static ShortUrl Reconstitute(Guid id, string originalUrl, string shortCode, 
        DateTime createdAt, int clickCount, bool isActive, DateTime? expiresAt = null)
}
```

#### ClickStatistic (Entity)
- Represents a single click event on a shortened URL
- Stores tracking information like user agent, IP address, and referer

```csharp
public class ClickStatistic : Entity
{
    public Guid ShortUrlId { get; private set; }
    public DateTime ClickedAt { get; private set; }
    public string? UserAgent { get; private set; }
    public string? IpAddress { get; private set; }
    public string? RefererUrl { get; private set; }
    
    // Factory method
    public static ClickStatistic Create(Guid shortUrlId, string? userAgent, 
        string? ipAddress, string? refererUrl)
}
```

#### AggregatedClickStatistic (Entity)
- Represents aggregated click data for storage optimization
- Stores summary data about clicks over a time period

#### CompressedShortUrl (Entity)
- Represents a compressed version of an expired or inactive ShortUrl
- Used for storage optimization

### Value Objects

- **UrlValidator**: Validates URLs according to domain rules
- **ShortUrlValidator**: Contains validation logic for short URLs

### Domain Events

- **UrlCreatedEvent**: Fired when a new short URL is created
- **UrlClickedEvent**: Fired when a short URL is clicked

## Application Layer

### Services

#### IShortUrlService
- Core service interface for URL shortening operations

```csharp
public interface IShortUrlService
{
    Task<ShortUrlDto> CreateShortUrlAsync(CreateShortUrlDto createShortUrlDto);
    Task<ShortUrlDto> GetShortUrlByCodeAsync(string shortCode);
    Task<string> RedirectAndTrackAsync(string shortCode);
    Task<IEnumerable<ShortUrlDto>> GetAllShortUrlsAsync();
    Task<ShortUrlDto> GetShortUrlDetailsByIdAsync(Guid id);
}
```

#### IStorageOptimizer
- Service for optimizing database storage by compressing old data

```csharp
public interface IStorageOptimizer
{
    Task<int> PurgeExpiredUrlsAsync();
    Task<int> CompressOldUrlDataAsync(TimeSpan olderThan);
    Task<int> AggregateOldClickStatisticsAsync(TimeSpan olderThan);
    Task<StorageStatistics> GetStorageStatisticsAsync();
}
```

#### IShortCodeGenerator
- Service for generating unique short codes

```csharp
public interface IShortCodeGenerator
{
    string GenerateShortCode(int length = 6);
}
```

### DTOs (Data Transfer Objects)

- **ShortUrlDto**: Representation of a ShortUrl entity for API responses
- **CreateShortUrlDto**: Input model for creating a new short URL
- **StorageStatistics**: Statistics about database storage

### CQRS Commands and Queries

#### Commands
- **CreateShortUrl.Command**: Creates a new short URL
- **RedirectAndTrack.Command**: Records a click and returns the original URL

#### Queries
- **GetShortUrlByCode.Query**: Retrieves a short URL by its code
- **GetAllShortUrls.Query**: Retrieves all short URLs

## Infrastructure Layer

### Repositories

#### IShortUrlRepository
- Repository interface for ShortUrl aggregate

```csharp
public interface IShortUrlRepository
{
    Task<ShortUrl> GetByIdAsync(Guid id);
    Task<ShortUrl> GetByShortCodeAsync(string shortCode);
    Task<IEnumerable<ShortUrl>> GetAllAsync();
    Task<ShortUrl> AddAsync(ShortUrl shortUrl);
    Task UpdateAsync(ShortUrl shortUrl);
    Task<bool> ShortCodeExistsAsync(string shortCode);
}
```

### Database

- **AppDbContext**: Entity Framework Core DbContext
- Configures entities and their relationships
- Handles database operations

### Background Services

- **StorageOptimizationBackgroundService**: Runs periodically to optimize storage

## API Layer

### Controllers

#### ShortUrlController
- REST API endpoints for URL shortening operations

```
- POST /api/ShortUrl - Create a new short URL
- GET /api/ShortUrl/{code} - Get a short URL by code
- GET /api/ShortUrl - Get all short URLs
- GET /api/ShortUrl/redirect/{code} - Redirect to the original URL
```

#### StorageController
- API endpoints for storage management

```
- GET /api/Storage/statistics - Get storage statistics
- POST /api/Storage/purge - Purge expired URLs
- POST /api/Storage/compress - Compress old URL data
- POST /api/Storage/aggregate - Aggregate old click statistics
```

### Middleware

- **ExceptionHandlingMiddleware**: Catches and handles exceptions
- **RateLimitingMiddleware**: Implements rate limiting for API endpoints

## Testing

The project contains extensive tests organized by layer:

### Domain Tests
- Tests for domain entities and their behaviors
- Tests for domain validation rules

### Application Tests
- Tests for application services
- Tests for CQRS commands and queries

### Infrastructure Tests
- Tests for repositories
- Tests for storage optimization

### API Tests
- Tests for controllers
- Tests for request/response handling

### Test Types

- **Unit Tests**: Test individual components in isolation
- **Integration Tests**: Test interactions between components
- **Mock Tests**: Use mock implementations for testing interactions

## Deployment

The URL Shortener application can be deployed using Docker or Kubernetes.

### Docker Deployment

The `deploy/docker` directory contains:
- **Dockerfile**: Multi-stage build for the application
- **docker-compose.yml**: Orchestrates the application and database
- **.dockerignore**: Excludes unnecessary files from the build context

```bash
# Build and run with Docker Compose
docker-compose -f deploy/docker/docker-compose.yml up -d
```

### Kubernetes Deployment

The `deploy/kubernetes` directory contains:
- **Deployments**: For the application and database
- **Services**: For internal communication
- **Ingress**: For external access
- **PVC**: For persistent storage
- **Secrets**: For sensitive configuration

```bash
# Deploy with Kubernetes
kubectl apply -k deploy/kubernetes
```

## Security Considerations

### Data Protection
- HTTPS for all communications
- Database connection string stored securely
- Password hashing for any user accounts

### Input Validation
- Validation of all input data
- Protection against SQL injection
- Protection against XSS attacks

### Rate Limiting
- Protection against abuse
- Configurable limits per IP address

## Performance Optimization

### Database Optimization
- Indexes on frequently queried columns
- Storage optimization for old data
- Partitioning strategies for large tables

### Application Optimization
- Response caching
- In-memory caching of frequently accessed data
- Asynchronous processing

### Monitoring and Health Checks
- Health endpoints for monitoring
- Performance metrics collection
- Alerting on performance degradation

## Monitoring and Logging

### Logging
- Structured logging with Serilog
- Log levels for different environments
- Log aggregation in production

### Metrics
- Request/response timings
- Database query performance
- Memory and CPU usage

### Health Checks
- Database connectivity
- External service availability
- Overall application health

## Extending the System

### Adding New Features
1. Implement domain entities and logic
2. Create application layer services and DTOs
3. Implement infrastructure components
4. Expose via API endpoints
5. Add tests for all layers

### Examples of Possible Extensions
- User accounts and authentication
- Custom domains for short URLs
- Enhanced analytics and reporting
- QR code generation
- API rate limiting tiers

### Integration with Other Systems
- Webhooks for click events
- Export to analytics platforms
- Social media integration 