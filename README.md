# Microservices Architecture

This repository contains a collection of microservices built with .NET 10 and PostgreSQL, including **AuthService**, **UserService**, and **PostService** for managing authentication, user profiles, and posts respectively.

## Technology Stack

- **Framework:** .NET 10 (C#)
- **Database:** PostgreSQL
- **ORM:** Dapper
- **Authentication:** JWT (System.IdentityModel.Tokens.Jwt)
- **Testing:** xUnit, Moq, FluentAssertions
- **API Documentation:** Swagger/OpenAPI

## API Documentation

Each service provides Swagger/OpenAPI documentation:

- **AuthService:** http://localhost:5003/swagger/index.html
- **UserService:** http://localhost:5010/swagger/index.html
- **PostService:** http://localhost:5000/swagger/index.html

## Microservices Overview

### 1. **AuthService** (Port 5003)
Handles all authentication and authorization operations.

**Endpoints:**
- `POST /auth/register` - Register a new user
- `POST /auth/login` - Login and receive JWT token
- `GET /auth/refreshtoken` - Refresh authentication token

### 2. **UserService** (Port 5001)
Manages user profile information and user-related operations.

**Endpoints:**
- `GET /user/getusers` - Get all users
- `GET /user/getsingleuser/{userId}` - Get specific user
- `PUT /user/edituser` - Update user profile
- `DELETE /user/deleteuser/{userId}` - Delete user
- `POST /user/adduser` - Add new user

### 3. **PostService** (Port 5000)
Manages user posts and content.

**Endpoints:**
- `GET /post/getposts` - Get all posts
- `GET /post/getsinglepost/{postId}` - Get specific post
- `POST /post/post` - Create new post
- `PUT /post/post` - Update post
- `DELETE /post/post/{postId}` - Delete post

#### Test Framework & Tools
- **xUnit:** Testing framework
- **Moq:** Mocking library for dependencies
- **FluentAssertions:** Fluent assertion syntax
- **In-Memory Configuration:** Mock IConfiguration for tests

### Tests

All services include comprehensive unit tests:

AuthService.Tests
UserService.Tests
PostService.Tests

## Project Structure

```
Microservices/
├── AuthService/              # Authentication microservice
│   ├── Controllers/
│   ├── Data/
│   │   ├── DataContextDapper.cs
│   │   └── IDataContextDapper.cs
│   ├── Dtos/
│   ├── Properties/
│   ├── Program.cs
│   ├── appsettings.json
│   └── README.md
├── AuthService.Tests/        # Unit tests for AuthService
│   ├── AuthControllerTests.cs
│   └── AuthService.Tests.csproj
├── UserService/              # User management microservice
│   ├── Controllers/
│   ├── Data/
│   │   ├── DataContextDapper.cs
│   │   └── IDataContextDapper.cs
│   ├── Dtos/
│   ├── Models/
│   ├── Properties/
│   ├── Program.cs
│   ├── appsettings.json
│   └── README.md
├── UserService.Tests/        # Unit tests for UserService
│   ├── UserControllerTest.cs
│   └── UserService.Tests.csproj
├── PostService/              # Post management microservice
│   ├── Controllers/
│   ├── Data/
│   │   ├── DataContextDapper.cs
│   │   └── IDataContextDapper.cs
│   ├── Dtos/
│   ├── Models/
│   ├── Properties/
│   ├── Program.cs
│   ├── appsettings.json
│   ├── TablePrep.sql
│   └── README.md
├── PostService.Tests/        # Unit tests for PostService
│   ├── PostControllerTest.cs
│   └── PostService.Tests.csproj
├── .vscode/
│   └── settings.json         # VS Code settings with test explorer config
├── Microservices.sln         # Master solution file
├── DATABASE_SETUP.md         # Database setup instructions
└── README.md                 # This file
```

## Getting Started

### Prerequisites

- .NET 10 SDK
- PostgreSQL 12+
- Visual Studio 2022 or VS Code

### Setup Instructions

#### 1. Clone/Navigate to Repository
```bash
cd /Users/madhuri.naragani/workspace/Microservices
```

#### 2. Setup Database
```bash
# Connect to PostgreSQL and run the schema setup
psql -U postgres -d microservices_db -f DatabaseSchema.sql
```

See [DATABASE_SETUP.md](./DATABASE_SETUP.md) for detailed instructions.

#### 3. Update Configuration
Update `appsettings.json` in each service with your database connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=5432;Database=microservices_db;User Id=postgres;Password=your_password;"
  }
}
```

**Important:** Update the `AppSettings:TokenKey` with a secure random string (at least 64 characters).

#### 4. Run Individual Services

**Terminal 1 - AuthService:**
```bash
cd AuthService
dotnet run
# Service runs on http://localhost:5003
```

**Terminal 2 - UserService:**
```bash
cd UserService
dotnet run
# Service runs on http://localhost:5001
```

**Terminal 3 - PostService:**
```bash
cd PostService
dotnet run
# Service runs on http://localhost:5000
```


```

#### Test Coverage

- **AuthControllerTests:** 10+ test cases covering Register, Login, and RefreshToken functionality
- **UserControllerTest:** 8 test cases covering GetSingleUser, GetUsers, AddUser, EditUser, DeleteUser
- **PostControllerTest:** Comprehensive tests for all Post operations

For detailed test coverage:
```bash
dotnet test /p:CollectCoverage=true
```

## Architecture Patterns

### Dependency Injection & Data Access
- **IDataContextDapper Interface:** Abstraction for all data access operations
- **DataContextDapper Implementation:** Concrete implementation using Dapper ORM
- **Constructor Injection:** All controllers receive dependencies via constructor
- **Benefits:** Loose coupling, testability, and flexibility

### Example Usage
```csharp
public class UserController : ControllerBase
{
    private readonly IDataContextDapper _dapper;
    
    public UserController(IDataContextDapper dapper)
    {
        _dapper = dapper;
    }
}

// Registered in Program.cs
builder.Services.AddScoped<IDataContextDapper, DataContextDapper>();
```

### Microservices Separation
- Each service has its own database
- Services communicate via HTTP/REST APIs
- Authentication is centralized in AuthService

### Data Access Layer
- Dapper ORM for database queries
- Parameterized queries to prevent SQL injection
- Async data operations

### Authentication & Authorization
- JWT (JSON Web Token) based authentication
- Token validation across all services
- Secure password hashing with PBKDF2

### Error Handling
- Comprehensive exception handling
- Appropriate HTTP status codes
- Detailed error messages in responses

## Configuration Files

### appsettings.json (All Services)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=5432;Database=microservices_db;"
  },
  "AppSettings": {
    "TokenKey": "Your secure 64+ character token key",
    "PasswordKey": "Your secure password salt"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

### launchSettings.json
Configures HTTP/HTTPS ports and environment for each service.

## Troubleshooting

### Port Already in Use
```bash
# Find and kill process using a specific port (macOS/Linux)
lsof -i :5000
kill -9 <PID>
```

### Database Connection Issues
1. Verify PostgreSQL is running
2. Check connection string in appsettings.json
3. Ensure database user has proper permissions
4. Verify database and tables exist

### JWT Token Issues
1. Ensure `TokenKey` is configured in all services
2. Verify token hasn't expired (24-hour expiration)
3. Check Authorization header format: `Authorization: Bearer <token>`

## Development Workflow

1. **Feature Development**
   - Develop feature in appropriate service
   - Write unit tests
   - Update API documentation

2. **Testing**
   - Run local tests: `dotnet test`
   - Test API endpoints using Swagger or Postman
   - Integration testing across services

3. **Deployment**
   - Build optimized release: `dotnet publish -c Release`
   - Update database schema if needed
   - Deploy services (order: AuthService → UserService → PostService)

## Security Considerations

- **Password Security:** PBKDF2 hashing with random salt
- **JWT Tokens:** HS512 encryption with 24-hour expiration
- **SQL Injection:** Parameterized queries throughout
- **CORS:** Configure as needed for frontend integration
- **HTTPS:** Enforce in production environment
