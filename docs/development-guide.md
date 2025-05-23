# URL Shortener Development Guide

This document provides guidelines and instructions for developers working on the URL Shortener project.

## Development Environment Setup

### Prerequisites

- **IDE**: Visual Studio 2022 or JetBrains Rider
- **.NET SDK**: .NET 8.0 or later
- **Database**: SQL Server 2019 or later (or SQL Server Express)
- **Docker** (optional): For containerized development
- **Git**: For source control

### Setting Up Local Environment

1. **Clone the Repository**:
   ```bash
   git clone https://github.com/yourusername/ShortenerUrl.git
   cd ShortenerUrl
   ```

2. **Install Dependencies**:
   ```bash
   dotnet restore
   ```

3. **Configure Local Database**:
   - Create a local SQL Server database named "URLShortenerDb"
   - Update the connection string in `appsettings.Development.json`
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=URLShortenerDb;Trusted_Connection=True;MultipleActiveResultSets=true"
   }
   ```

4. **Apply Database Migrations**:
   ```bash
   cd URLShortener.API
   dotnet ef database update
   ```

5. **Run the Application**:
   ```bash
   dotnet run
   ```

## Project Structure

The URL Shortener follows Clean Architecture principles with the following structure:

```
ShortenerUrl/
├── URLShortener.API/              # Web API project
├── URLShortener.Application/      # Application services, DTOs, CQRS
├── URLShortener.Domain/           # Domain entities and business logic
├── URLShortener.Infrastructure/   # Data access, external services
├── URLShortener.Tests/            # Test projects
└── deploy/                        # Deployment configurations
```

### Key Components

- **Domain Layer**: Contains the core business entities and logic
  - `ShortUrl.cs`, `ClickStatistic.cs`, etc.
  
- **Application Layer**: Contains the application services and CQRS commands/queries
  - `IShortUrlService.cs`, `ShortUrlService.cs`
  - CQRS commands and queries in `Commands/` and `Queries/` folders
  
- **Infrastructure Layer**: Contains data access implementations
  - `AppDbContext.cs`
  - `ShortUrlRepository.cs`
  
- **API Layer**: Contains API controllers and configuration
  - `ShortUrlController.cs`
  - `Startup.cs`

## Coding Standards

### Naming Conventions

- **Classes**: PascalCase (e.g., `ShortUrlService`)
- **Methods**: PascalCase (e.g., `CreateShortUrl`)
- **Interfaces**: Prefix with "I" (e.g., `IShortUrlRepository`)
- **Private Fields**: camelCase with underscore prefix (e.g., `_repository`)
- **Properties**: PascalCase (e.g., `OriginalUrl`)
- **Method Parameters**: camelCase (e.g., `shortCode`)

### Code Organization

- Follow the principles of Clean Architecture
- Keep domain entities focused on business logic
- Use CQRS pattern for operations (MediatR)
- Follow the Dependency Inversion Principle

### Error Handling

- Use domain-specific exceptions in the domain layer
- Handle exceptions at the application layer
- Return appropriate HTTP status codes from the API

### Asynchronous Programming

- Use async/await for all I/O operations
- Ensure proper task propagation
- Avoid blocking calls

## Adding New Features

### 1. Domain Changes

Start by updating the domain model:

1. Add or modify entities in `URLShortener.Domain/Entities/`
2. Add domain validation logic
3. Update domain services if needed

Example of adding a new entity:

```csharp
// URLShortener.Domain/Entities/MyNewEntity.cs
public class MyNewEntity : Entity
{
    public string Name { get; private set; }
    public DateTime CreatedAt { get; private set; }
    
    private MyNewEntity() { } // For EF Core
    
    public MyNewEntity(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        CreatedAt = DateTime.UtcNow;
    }
    
    public void UpdateName(string newName)
    {
        Name = newName ?? throw new ArgumentNullException(nameof(newName));
    }
}
```

### 2. Application Layer Changes

Update application services and DTOs:

1. Create DTOs in `URLShortener.Application/DTOs/`
2. Add service interfaces and implementations
3. Create CQRS commands and queries

Example of adding a CQRS command:

```csharp
// URLShortener.Application/CQRS/Commands/CreateMyEntity/CreateMyEntityCommand.cs
public class CreateMyEntityCommand : IRequest<Guid>
{
    public string Name { get; set; }
}

// URLShortener.Application/CQRS/Commands/CreateMyEntity/CreateMyEntityCommandHandler.cs
public class CreateMyEntityCommandHandler : IRequestHandler<CreateMyEntityCommand, Guid>
{
    private readonly IMyEntityRepository _repository;
    
    public CreateMyEntityCommandHandler(IMyEntityRepository repository)
    {
        _repository = repository;
    }
    
    public async Task<Guid> Handle(CreateMyEntityCommand request, CancellationToken cancellationToken)
    {
        var entity = new MyNewEntity(request.Name);
        await _repository.AddAsync(entity);
        return entity.Id;
    }
}
```

### 3. Infrastructure Layer Changes

Update data access and external services:

1. Update `AppDbContext.cs` to include new entities
2. Add repository implementations
3. Add EF Core migrations

Example of updating the DbContext:

```csharp
// URLShortener.Infrastructure/Data/AppDbContext.cs
public class AppDbContext : DbContext
{
    // Existing code...
    
    public DbSet<MyNewEntity> MyNewEntities { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Existing code...
        
        modelBuilder.Entity<MyNewEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CreatedAt).IsRequired();
        });
    }
}
```

Creating a migration:

```bash
cd URLShortener.API
dotnet ef migrations add AddMyNewEntity
```

### 4. API Layer Changes

Add API endpoints:

1. Create or update controller(s)
2. Add appropriate routing and HTTP methods
3. Implement proper request validation and error handling

Example of adding an API endpoint:

```csharp
// URLShortener.API/Controllers/MyEntityController.cs
[ApiController]
[Route("api/[controller]")]
public class MyEntityController : ControllerBase
{
    private readonly IMediator _mediator;
    
    public MyEntityController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateMyEntityCommand command)
    {
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }
    
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(MyEntityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetMyEntityByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        
        if (result == null)
            return NotFound();
            
        return Ok(result);
    }
}
```

### 5. Testing

Add tests for new functionality:

1. Unit tests for domain logic
2. Unit tests for application services
3. Integration tests for repositories
4. API tests for endpoints

Example of a unit test:

```csharp
// URLShortener.Tests/Domain/MyNewEntityTests.cs
public class MyNewEntityTests
{
    [Fact]
    public void Create_WithValidName_SetsNameAndCreatedAt()
    {
        // Arrange
        var name = "Test Entity";
        
        // Act
        var entity = new MyNewEntity(name);
        
        // Assert
        Assert.Equal(name, entity.Name);
        Assert.InRange(entity.CreatedAt, DateTime.UtcNow.AddSeconds(-5), DateTime.UtcNow);
    }
    
    [Fact]
    public void Create_WithNullName_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new MyNewEntity(null));
    }
}
```

## Testing

### Running Tests

To run all tests:

```bash
dotnet test
```

To run specific test projects:

```bash
dotnet test URLShortener.Tests/URLShortener.Tests.csproj
```

### Test Data

- Use in-memory database for repository tests
- Use mocks for external dependencies
- Create test data factories for common entities

### Test Categories

- **Unit Tests**: Test individual components in isolation
- **Integration Tests**: Test interactions between components
- **API Tests**: Test API endpoints

## Database Migrations

### Creating a Migration

```bash
cd URLShortener.API
dotnet ef migrations add <MigrationName>
```

### Applying Migrations

```bash
dotnet ef database update
```

### Reverting Migrations

```bash
dotnet ef database update <PreviousMigrationName>
```

## Best Practices

### Performance Optimization

- Use asynchronous operations
- Implement caching for frequently accessed data
- Add indexes to database tables for common queries
- Use pagination for large data sets

### Security

- Validate all input data
- Use parameterized queries
- Implement rate limiting
- Use HTTPS for all communications
- Follow the principle of least privilege

### Logging

- Use structured logging with Serilog
- Log appropriate information at each level
- Don't log sensitive information
- Include correlation IDs for request tracking

## Troubleshooting

### Common Issues

#### Database Connection Issues

- Check connection string in `appsettings.json`
- Ensure SQL Server is running
- Verify database exists and permissions are correct

#### Entity Framework Migrations

- If migrations fail, check if the database state matches the migration history
- Try `dotnet ef database drop` followed by `dotnet ef database update` to reset

#### API Requests

- Use Swagger UI for testing API endpoints
- Check request payload matches expected format
- Verify authentication/authorization if applicable

## Contribution Guidelines

### Pull Requests

1. Create a feature branch from `develop`
2. Implement changes with appropriate tests
3. Ensure all tests pass
4. Submit a pull request to `develop`
5. Wait for code review and approval

### Commit Messages

Follow the conventional commits format:

- `feat: add new feature`
- `fix: correct bug in xyz`
- `docs: update documentation`
- `test: add test for feature`
- `refactor: improve code structure`

### Code Reviews

- All code must be reviewed before merging
- Reviewers should check for:
  - Functionality
  - Test coverage
  - Code style
  - Performance considerations
  - Security implications

## Resources

- [.NET Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [CQRS Pattern](https://docs.microsoft.com/en-us/azure/architecture/patterns/cqrs)
- [MediatR Documentation](https://github.com/jbogard/MediatR/wiki) 