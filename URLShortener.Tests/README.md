# URL Shortener Tests

This project contains comprehensive test coverage for the URL Shortener application, following best practices for testing each layer of the application.

## Testing Strategy

The tests are organized by architectural layer:

- **Domain Tests**: Test the core domain entities and their business logic
- **Application Tests**: Test the application services and use cases
- **Infrastructure Tests**: Test the data access and external services
- **API Tests**: Test the API controllers and endpoints

## Test Coverage

The test suite provides coverage for:

- Core domain logic (entity creation, validation, business rules)
- Application services (ShortUrlService, validation, etc.)
- Data access (Repository pattern)
- Storage optimization (compression, aggregation, purging)
- API controllers (request/response handling, error cases)
- Edge cases and error handling

## Test Technologies

- **xUnit**: Testing framework
- **Moq**: Mocking framework for isolating components
- **FluentAssertions**: Fluent API for assertions
- **Entity Framework Core InMemory**: For testing data access without a real database
- **Coverlet**: For measuring code coverage

## Running Tests

### Using Visual Studio

1. Open the solution in Visual Studio
2. Right-click on the `URLShortener.Tests` project
3. Select "Run Tests"

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

Code coverage reports are generated in the Cobertura format, which can be integrated with most CI/CD pipelines and code quality tools.

## Best Practices Followed

- **Arrange-Act-Assert**: Tests follow the AAA pattern
- **Isolation**: Components are properly isolated using mocks and stubs
- **Naming**: Tests are named according to `MethodName_Scenario_ExpectedResult` pattern
- **Single Responsibility**: Each test verifies a single behavior
- **Comprehensive Coverage**: Tests cover happy paths, edge cases, and error scenarios 