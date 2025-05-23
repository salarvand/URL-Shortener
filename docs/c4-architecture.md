# C4 Architecture Diagrams for URL Shortener

This document provides C4 model diagrams for the URL Shortener system, showing different levels of abstraction from context to container level.

## C4 Model Introduction

The C4 model is a way of describing and communicating software architecture through a set of hierarchical diagrams:
- **Context**: Shows how the system fits into the world around it
- **Container**: Shows the high-level technical building blocks
- **Component**: Shows how containers are made up of components
- **Code**: Shows how components are implemented

## 1. System Context Diagram

```
┌───────────────────────────────────────────────────────────────────┐
│                                                                   │
│                          External World                           │
│                                                                   │
└───────────────────────────────────────────────────────────────────┘
          │                          │                    │
          │                          │                    │
          ▼                          ▼                    ▼
┌─────────────────────┐   ┌────────────────────┐   ┌────────────────────┐
│                     │   │                    │   │                    │
│     End Users       │   │   External APIs    │   │   Admin Users      │
│                     │   │                    │   │                    │
└─────────────────────┘   └────────────────────┘   └────────────────────┘
          │                          │                    │
          │                          │                    │
          ▼                          ▼                    ▼
┌───────────────────────────────────────────────────────────────────┐
│                                                                   │
│                      URL Shortener System                         │
│                                                                   │
│  Creates short URLs, tracks clicks, and provides analytics        │
│                                                                   │
└───────────────────────────────────────────────────────────────────┘
```

### Context Description

- **End Users**: People who create short URLs and use them for sharing
- **External APIs**: Systems that integrate with the URL Shortener via API
- **Admin Users**: Users who manage the system and view analytics
- **URL Shortener System**: The core system that provides URL shortening and tracking functionality

## 2. Container Diagram

```
┌───────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│                                                         URL Shortener System                                               │
│                                                                                                                           │
│ ┌─────────────────────────────┐       ┌──────────────────────────────┐       ┌────────────────────────────────────────┐   │
│ │                             │       │                              │       │                                        │   │
│ │       Web Application       │◄──────┤         API Layer           │◄──────┤            Frontend SPA                │   │
│ │                             │       │                              │       │                                        │   │
│ │   ASP.NET Core MVC Views    │       │      ASP.NET Core API        │       │          JavaScript/React              │   │
│ │                             │       │                              │       │                                        │   │
│ └─────────────────────────────┘       └──────────────────────────────┘       └────────────────────────────────────────┘   │
│           │         ▲                        │           ▲                                   │          ▲                  │
│           │         │                        │           │                                   │          │                  │
│           ▼         │                        ▼           │                                   ▼          │                  │
│ ┌─────────────────────────────┐       ┌──────────────────────────────┐       ┌────────────────────────────────────────┐   │
│ │                             │       │                              │       │                                        │   │
│ │    Application Layer        │◄──────┤      Domain Layer            │◄──────┤        Background Services             │   │
│ │                             │       │                              │       │                                        │   │
│ │  CQRS (MediatR), Services   │       │    Entities, Aggregates      │       │    Storage Optimization, Scheduled     │   │
│ │                             │       │                              │       │                Jobs                    │   │
│ └─────────────────────────────┘       └──────────────────────────────┘       └────────────────────────────────────────┘   │
│           │                                      │                                             │                          │
│           │                                      │                                             │                          │
│           ▼                                      ▼                                             ▼                          │
│ ┌─────────────────────────────┐       ┌──────────────────────────────┐       ┌────────────────────────────────────────┐   │
│ │                             │       │                              │       │                                        │   │
│ │     Data Access Layer       │◄──────┤        SQL Database          │◄──────┤        Caching Layer                   │   │
│ │                             │       │                              │       │                                        │   │
│ │  Entity Framework Core      │       │       MS SQL Server          │       │        Redis / In-Memory               │   │
│ │                             │       │                              │       │                                        │   │
│ └─────────────────────────────┘       └──────────────────────────────┘       └────────────────────────────────────────┘   │
│                                                                                                                           │
└───────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
```

### Container Descriptions

#### Frontend Containers
1. **Frontend SPA**
   - **Responsibility**: User interface for creating and managing short URLs
   - **Technology**: JavaScript, React
   - **Interactions**: Communicates with API Layer

2. **Web Application**
   - **Responsibility**: Server-rendered views for URL redirection and non-SPA pages
   - **Technology**: ASP.NET Core MVC
   - **Interactions**: Communicates with Application Layer

3. **API Layer**
   - **Responsibility**: RESTful API endpoints for URL operations
   - **Technology**: ASP.NET Core API Controllers
   - **Interactions**: Communicates with Application Layer

#### Core Containers
4. **Application Layer**
   - **Responsibility**: Application logic, use cases, and orchestration
   - **Technology**: CQRS pattern with MediatR
   - **Interactions**: Uses Domain Layer for business logic, communicates with Data Access Layer

5. **Domain Layer**
   - **Responsibility**: Core business logic, entities, and rules
   - **Technology**: .NET Domain objects, value objects
   - **Interactions**: Used by Application Layer

6. **Background Services**
   - **Responsibility**: Long-running tasks, cleanup, and maintenance
   - **Technology**: ASP.NET Core Hosted Services
   - **Interactions**: Uses Application Layer for operations

#### Data Containers
7. **Data Access Layer**
   - **Responsibility**: Database operations and object-relational mapping
   - **Technology**: Entity Framework Core, Repository pattern
   - **Interactions**: Communicates with SQL Database

8. **SQL Database**
   - **Responsibility**: Persistent storage of URL and click data
   - **Technology**: Microsoft SQL Server
   - **Interactions**: Stores data for all other containers

9. **Caching Layer**
   - **Responsibility**: Temporary storage for frequently accessed data
   - **Technology**: Redis or ASP.NET Core In-Memory Cache
   - **Interactions**: Provides fast data access for Application Layer

## 3. Container Interactions

### Key Interactions

1. **URL Creation Flow**
   ```
   Frontend SPA → API Layer → Application Layer → Domain Layer → Data Access Layer → SQL Database
   ```

2. **URL Redirection Flow**
   ```
   Web Application → Application Layer → Domain Layer → Data Access Layer → SQL Database
   ```
   With caching:
   ```
   Web Application → Application Layer → Caching Layer → [if miss] → Data Access Layer → SQL Database
   ```

3. **Click Tracking Flow**
   ```
   Web Application → Application Layer → Domain Layer → Background Services → Data Access Layer → SQL Database
   ```

4. **Storage Optimization Flow**
   ```
   Background Services → Application Layer → Domain Layer → Data Access Layer → SQL Database
   ```

## 4. Technology Stack

### Frontend Technologies
- **Frontend SPA**: React, JavaScript/TypeScript, HTML5, CSS3
- **Web Application**: ASP.NET Core MVC, Razor Views

### Backend Technologies
- **API Layer**: ASP.NET Core Web API
- **Application Layer**: MediatR, FluentValidation, AutoMapper
- **Domain Layer**: C# Domain Models
- **Background Services**: IHostedService, BackgroundService

### Data Technologies
- **Data Access Layer**: Entity Framework Core, Repository Pattern
- **SQL Database**: Microsoft SQL Server
- **Caching Layer**: Redis or ASP.NET Core IMemoryCache

### Infrastructure Technologies
- **Containerization**: Docker
- **Orchestration**: Kubernetes
- **Logging**: Serilog, Application Insights
- **Authentication**: JWT, ASP.NET Core Identity

## 5. Deployment View

```
┌─────────────────────────────────────────────────────────────────────┐
│                      Kubernetes Cluster                              │
│                                                                     │
│  ┌─────────────────┐      ┌─────────────────┐     ┌──────────────┐  │
│  │                 │      │                 │     │              │  │
│  │   API Pods      │◄─────┤   Web Pods      │     │  Redis Pod   │  │
│  │                 │      │                 │     │              │  │
│  └─────────────────┘      └─────────────────┘     └──────────────┘  │
│          │                        │                      │          │
│          ▼                        ▼                      ▼          │
│  ┌──────────────────────────────────────────────────────────────┐   │
│  │                                                              │   │
│  │                    Ingress Controller                        │   │
│  │                                                              │   │
│  └──────────────────────────────────────────────────────────────┘   │
│                              │                                       │
│                              ▼                                       │
│  ┌──────────────────────────────────────────────────────────────┐   │
│  │                                                              │   │
│  │                SQL Server StatefulSet                        │   │
│  │                                                              │   │
│  └──────────────────────────────────────────────────────────────┘   │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

### Deployment Components

- **API Pods**: Stateless deployments of the API Layer
- **Web Pods**: Stateless deployments of the Web Application
- **Redis Pod**: Cache service for performance optimization
- **Ingress Controller**: Manages external access to the services
- **SQL Server StatefulSet**: Persistent database with attached storage

## Conclusion

The C4 model diagrams provide a comprehensive view of the URL Shortener system architecture from high-level context to detailed container interactions. This architecture follows clean architecture principles with clear separation of concerns and dependencies flowing inward toward the domain layer.

The system is designed to be scalable, maintainable, and deployable in modern containerized environments while providing a robust URL shortening service with analytics capabilities. 