# Microservices Architecture

This repository contains a collection of microservices built with .NET 10 and PostgreSQL.

## Microservices Overview

### 1. **AuthService** (Port 5003)
Handles all authentication and authorization operations.

**Endpoints:**
- `POST /auth/register` - Register a new user
- `POST /auth/login` - Login and receive JWT token
- `GET /auth/refreshtoken` - Refresh authentication token

**Tech Stack:** ASP.NET Core 10, PostgreSQL, JWT, Dapper

[AuthService README](./AuthService/README.md)

### 2. **UserService** (Port 5001)
Manages user profile information and user-related operations.

**Endpoints:**
- `GET /user/getusers` - Get all users
- `GET /user/getsingleuser/{userId}` - Get specific user
- `PUT /user/edituser` - Update user profile
- `DELETE /user/deleteuser/{userId}` - Delete user
- `POST /user/adduser` - Add new user

**Tech Stack:** ASP.NET Core 10, PostgreSQL, Dapper

[UserService README](./UserService/README.md)

### 3. **PostService** (Port 5000)
Manages user posts and content.

**Endpoints:**
- `GET /post/getposts` - Get all posts
- `GET /post/getsinglepost/{postId}` - Get specific post
- `POST /post/post` - Create new post
- `PUT /post/post` - Update post
- `DELETE /post/post/{postId}` - Delete post

**Tech Stack:** ASP.NET Core 10, PostgreSQL, Dapper

[PostService README](./PostService/README.md)

## Project Structure

```
Microservices/
├── AuthService/              # Authentication microservice
│   ├── Controllers/
│   ├── Data/
│   ├── Dtos/
│   ├── Properties/
│   ├── Program.cs
│   ├── appsettings.json
│   └── README.md
├── UserService/              # User management microservice
│   ├── Controllers/
│   ├── Data/
│   ├── Dtos/
│   ├── Models/
│   ├── Properties/
│   ├── Program.cs
│   ├── appsettings.json
│   └── README.md
├── PostService/              # Post management microservice
│   ├── Controllers/
│   ├── Data/
│   ├── Dtos/
│   ├── Models/
│   ├── Properties/
│   ├── Program.cs
│   ├── appsettings.json
│   ├── TablePrep.sql
│   └── README.md
├── PostService.Tests/        # Unit tests for PostService
│   ├── Controllers/
│   ├── Models/
│   ├── Data/
│   ├── Integration/
│   └── PostService.Tests.csproj
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

### Running Tests

```bash
cd PostService.Tests
dotnet test
```

For detailed test coverage:
```bash
dotnet test /p:CollectCoverage=true
```

## API Documentation

Each service provides Swagger/OpenAPI documentation:

- **AuthService:** https://localhost:7003/swagger/index.html
- **UserService:** https://localhost:7001/swagger/index.html
- **PostService:** https://localhost:7000/swagger/index.html

## Architecture Patterns

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

## Technology Stack

- **Framework:** .NET 10 (C#)
- **Database:** PostgreSQL
- **ORM:** Dapper
- **Authentication:** JWT (System.IdentityModel.Tokens.Jwt)
- **Testing:** xUnit, Moq
- **API Documentation:** Swagger/OpenAPI

## Future Enhancements

- [ ] API Gateway implementation
- [ ] Service discovery (Consul/Eureka)
- [ ] Circuit breaker pattern
- [ ] Event-driven architecture (message queues)
- [ ] Distributed tracing
- [ ] Centralized logging (ELK stack)
- [ ] Rate limiting and throttling
- [ ] caching layer (Redis)

## Contributing

1. Create a feature branch
2. Make your changes
3. Write/update tests
4. Ensure all tests pass
5. Submit pull request

## License

This project is licensed under the MIT License.

## Support

For issues or questions:
1. Check the service-specific README
2. Review DATABASE_SETUP.md for database issues
3. Check Swagger documentation for API details
4. Review test files for usage examples
