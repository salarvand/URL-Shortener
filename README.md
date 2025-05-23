# URL Shortener

A modern, scalable URL shortening service built with ASP.NET Core, following Clean Architecture principles and CQRS pattern.

[![Build Status](https://img.shields.io/github/workflow/status/yourusername/ShortenerUrl/CI)](https://github.com/yourusername/ShortenerUrl/actions)
[![License](https://img.shields.io/github/license/yourusername/ShortenerUrl)](LICENSE)

## 🚀 Features

- **URL Shortening**: Create shortened URLs with custom or auto-generated codes
- **Click Tracking**: Record and analyze URL usage statistics
- **Expiration Dates**: Set expiration dates for temporary links
- **Storage Optimization**: Automatic cleanup and compression of old data
- **REST API**: Full API access for integration with other systems
- **Performance**: Optimized for high-volume traffic with caching

## 📋 Table of Contents

- [Architecture](#architecture)
- [Getting Started](#getting-started)
- [Project Structure](#project-structure)
- [API Reference](#api-reference)
- [Deployment](#deployment)
- [Database](#database)
- [Development](#development)
- [Testing](#testing)
- [Documentation](#documentation)
- [Technologies](#technologies)
- [License](#license)

## 🏗️ Architecture

This project follows Clean Architecture principles and Domain-Driven Design, organized into four main layers:

- **Domain Layer**: Core business entities and logic
- **Application Layer**: Use cases and orchestration with CQRS pattern
- **Infrastructure Layer**: Database access, external services
- **API Layer**: REST API endpoints

For a detailed architectural overview, including C4 diagrams, see [Architecture Documentation](docs/c4-architecture.md).

## 🚦 Getting Started

### Prerequisites

- [.NET SDK 8.0](https://dotnet.microsoft.com/download) or later
- [SQL Server 2019](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) or later
- [Docker](https://www.docker.com/products/docker-desktop) (optional, for containerized deployment)

### Local Development Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/ShortenerUrl.git
   cd ShortenerUrl
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Set up the database**
   ```bash
   cd URLShortener.API
   dotnet ef database update
   ```

4. **Run the application**
   ```bash
   dotnet run
   ```

5. **Access the application**
   - Web Interface: `https://localhost:5001`
   - API: `https://localhost:5001/api`
   - Swagger: `https://localhost:5001/swagger`

For more detailed setup instructions, see the [Development Guide](docs/development-guide.md).

## 📁 Project Structure

```
ShortenerUrl/
├── URLShortener.API/              # Web API and presentation layer
├── URLShortener.Application/      # Application services and CQRS
├── URLShortener.Domain/           # Domain entities and business rules
├── URLShortener.Infrastructure/   # Data access and external services
├── URLShortener.Tests/            # Unit and integration tests
├── URLShortener.UI/               # React-based frontend (optional)
├── docs/                          # Project documentation
└── deploy/                        # Deployment configurations
```

## 🔌 API Reference

The URL Shortener provides a RESTful API for all operations:

- `POST /api/ShortUrl` - Create a new short URL
- `GET /api/ShortUrl/{code}` - Get information about a short URL
- `GET /api/ShortUrl/redirect/{code}` - Redirect to the original URL
- `GET /api/Storage/statistics` - Get storage statistics

For a complete API reference with examples, see the [API Documentation](docs/api-reference.md).

## 🚢 Deployment

The application can be deployed using:

### Docker

```bash
docker-compose -f deploy/docker/docker-compose.yml up -d
```

### Kubernetes

```bash
kubectl apply -k deploy/kubernetes
```

### Manual Deployment

Detailed instructions for IIS, Nginx, and other platforms are available in the [Deployment Guide](docs/deployment-guide.md).

## 💾 Database

The URL Shortener uses SQL Server with the following main tables:

- `ShortUrls` - Stores URL mappings
- `ClickStatistics` - Records click events
- `AggregatedClickStats` - Stores optimized historical data
- `CompressedShortUrls` - Archives old URLs

See the [Database Schema Documentation](docs/database-schema.md) for details including schema diagrams, indexes, and query examples.

## 👨‍💻 Development

### Adding New Features

To add new features to the URL Shortener:

1. Define the domain model updates if needed
2. Create application layer command/query handlers
3. Update the API endpoints
4. Add appropriate tests
5. Submit a pull request

Follow our coding standards and architecture principles detailed in the [Development Guide](docs/development-guide.md).

## 🧪 Testing

The project includes various test types:

- Unit tests for domain logic
- Integration tests for API endpoints
- Performance tests for critical operations

Run all tests with:

```bash
dotnet test
```

## 📚 Documentation

Comprehensive documentation is available in the [docs](docs/) directory:

- [API Reference](docs/api-reference.md)
- [Architecture Overview](docs/c4-architecture.md)
- [Database Schema](docs/database-schema.md)
- [Deployment Guide](docs/deployment-guide.md)
- [Development Guide](docs/development-guide.md)
- [User Guide](docs/user-guide.md)

## 🔧 Technologies

- **Backend**: ASP.NET Core 8, C# 12
- **ORM**: Entity Framework Core 8
- **Database**: Microsoft SQL Server
- **Architecture**: Clean Architecture, CQRS with MediatR
- **Validation**: FluentValidation
- **Testing**: xUnit, FluentAssertions, Moq
- **Containerization**: Docker, Kubernetes
- **Documentation**: Swagger / OpenAPI

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgements

- Clean Architecture principles by Robert C. Martin
- CQRS pattern implementation inspired by Jimmy Bogard

---

For questions and support, please [open an issue](https://github.com/yourusername/ShortenerUrl/issues) on the GitHub repository.