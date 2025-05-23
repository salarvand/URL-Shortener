# URL Shortener Testing Strategy

This document outlines the comprehensive testing strategy for the URL Shortener application, focusing on ensuring code quality, reliability, and maintainability.

## Testing Approach

The testing approach follows a multi-layered strategy, covering all aspects of the application:

### 1. Unit Testing

Unit tests focus on testing individual components in isolation:

- **Domain Layer Tests**: Validate the core business logic and entity behaviors
  - Entity creation, validation rules, and business operations
  - Value objects and domain services
  - Domain events and behaviors

- **Application Layer Tests**: Verify application services and use cases
  - Command and query handlers
  - Service implementations
  - Validation logic and error handling

- **Infrastructure Layer Tests**: Test infrastructure implementations
  - Repository implementations
  - External service integrations
  - Data access and persistence

- **API Layer Tests**: Validate controllers and endpoints
  - Request/response handling
  - Status codes and error responses
  - Authentication and authorization

### 2. Integration Testing

Integration tests verify that components work together correctly:

- **Repository Integration Tests**: Test repositories with actual database
- **API Integration Tests**: Test API endpoints with the full request pipeline
- **Service Integration Tests**: Verify service interactions

### 3. End-to-End Testing

End-to-end tests validate complete user scenarios:

- URL creation and redirection flows
- Analytics collection and reporting
- Administrative operations

## Test Coverage Goals

The project aims for high test coverage across all layers:

- **Domain Layer**: 90%+ coverage
- **Application Layer**: 85%+ coverage
- **Infrastructure Layer**: 75%+ coverage
- **API Layer**: 80%+ coverage

## Testing Tools and Technologies

The test suite uses the following tools:

- **xUnit**: Primary testing framework
- **Moq**: Mocking framework for isolating components
- **FluentAssertions**: Fluent API for more readable assertions
- **Entity Framework Core InMemory**: For testing data access without a real database
- **Coverlet**: For measuring code coverage

## Test Categories

Tests are organized into the following categories:

1. **Fast Tests**: Quick-running unit tests that don't require external dependencies
2. **Slow Tests**: Integration and E2E tests that may require database setup
3. **Security Tests**: Tests focusing on security aspects like URL scanning and rate limiting
4. **Performance Tests**: Tests for storage optimization and performance benchmarks

## Testing Storage Optimization

Special attention is given to testing the storage optimization features:

- **Purging Expired URLs**: Verify that expired URLs are correctly purged and compressed
- **Click Statistics Aggregation**: Test the aggregation of old click statistics
- **URL Compression**: Validate the compression and decompression of URL data
- **Storage Metrics**: Test the accuracy of storage statistics reporting

## Test Data Management

Test data is managed through:

- **Test Data Factory**: Centralized creation of test entities
- **In-Memory Database**: For integration tests without external dependencies
- **Test Fixtures**: For sharing setup between related tests

## Continuous Integration

Tests are integrated into the CI/CD pipeline:

- All tests run on every pull request
- Code coverage reports are generated and tracked
- Performance benchmarks are monitored for regressions

## Test Naming Convention

Tests follow a consistent naming convention:

```
[MethodName]_[Scenario]_[ExpectedResult]
```

Examples:
- `CreateShortUrl_WithValidInput_ReturnsShortUrlDto`
- `GetByShortCode_WithNonExistingCode_ReturnsNotFound`

## Best Practices

The testing approach follows these best practices:

- **Arrange-Act-Assert**: Clear separation of test phases
- **Single Responsibility**: Each test verifies one behavior
- **Test Independence**: Tests do not depend on each other
- **Realistic Data**: Tests use realistic data scenarios
- **Edge Cases**: Tests cover boundary conditions and error cases
- **Clean Tests**: Tests are readable and maintainable

## Running Tests

### Using Visual Studio

1. Open the solution in Visual Studio
2. Use Test Explorer to run individual tests or test categories
3. View code coverage results in the Coverage window

### Using Command Line

```bash
# Run all tests
dotnet test

# Run tests with coverage report
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura

# Run specific test category
dotnet test --filter "Category=Domain"
```

## Test Reports

Test reports are generated in the following formats:

- **Cobertura**: XML coverage reports for CI integration
- **HTML**: Human-readable coverage reports
- **JUnit**: Test execution reports 