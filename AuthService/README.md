# AuthService Microservice

The AuthService is a dedicated microservice for handling all authentication and authorization operations in the microservices architecture.

## Features

- User registration with email and password
- User login with JWT token generation
- Token refresh functionality
- Password hashing using PBKDF2
- Secure password salt generation
- JWT-based authentication

## Project Structure

```
AuthService/
├── Controllers/
│   └── AuthController.cs       # Authentication endpoints
├── Data/
│   └── DataContextDapper.cs    # Database access layer
├── Dtos/
│   ├── UserForRegistrationDto.cs
│   ├── UserForLoginDto.cs
│   └── UserForLoginConfirmationDto.cs
├── Properties/
│   └── launchSettings.json
├── Program.cs                  # Application configuration
├── appsettings.json            # Default settings
├── appsettings.Development.json # Development settings
└── DotnetAPI.csproj            # Project file
```

## Endpoints

### Register User
**POST** `/auth/register`

Request body:
```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!",
  "passwordConfirm": "SecurePassword123!",
  "firstName": "John",
  "lastName": "Doe",
  "gender": "Male"
}
```

Response: `200 OK`

### Login
**POST** `/auth/login`

Request body:
```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!"
}
```

Response:
```json
{
  "token": "eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9..."
}
```

### Refresh Token
**GET** `/auth/refreshtoken`

Headers:
```
Authorization: Bearer <existing_token>
```

Response:
```json
{
  "token": "eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9..."
}
```

## Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=5432;Database=microservices_db;User Id=postgres;Password=password;"
  },
  "AppSettings": {
    "TokenKey": "This is my super secret key that needs to be at least 64 characters long for the 256 bits",
    "PasswordKey": "This is a password salt key that is super secret"
  }
}
```

## Running the Service

```bash
cd AuthService
dotnet run
```

The service will start on:
- HTTP: `http://localhost:5003`
- HTTPS: `https://localhost:7003`

## Database Requirements

The AuthService requires the following tables:
- `public.Auth` - Stores authentication credentials (Email, PasswordHash, PasswordSalt)
- `public.Users` - Stores user profile information

See [DATABASE_SETUP.md](../DATABASE_SETUP.md) for schema setup instructions.

## Security Features

- **Password Hashing**: PBKDF2 with HMAC-SHA256
- **JWT Tokens**: HS512 signing algorithm
- **Token Expiration**: 24 hours
- **Password Salt**: Cryptographically secure random salt generation
- **Parameterized Queries**: Protection against SQL injection

## Integration with Other Services

Other microservices can verify JWT tokens using the same `TokenKey` configured in the `AppSettings`.

Example verification in another service:
```csharp
var tokenKey = configuration.GetSection("AppSettings:TokenKey").Value;
var tokenKeyBytes = Encoding.UTF8.GetBytes(tokenKey);

services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(tokenKeyBytes),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });
```

## Error Handling

Common error responses:

- `400 Bad Request` - Invalid input or password mismatch
- `401 Unauthorized` - Incorrect password
- `409 Conflict` - User already exists
- `500 Internal Server Error` - Database or configuration error

## Migration from PostService

The AuthService was extracted from PostService to follow microservices best practices. The original AuthController in PostService is now deprecated and returns `NotImplementedException`.

Update your service configurations to use AuthService endpoints instead:
- Old: `http://localhost:5000/auth/login`
- New: `http://localhost:5003/auth/login`
