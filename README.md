# ASP.NET Task Manager API

A RESTful API for task management built with ASP.NET Core 8.0, featuring JWT authentication and SQL Server database.

## Features

- User authentication with JWT tokens
- Task CRUD operations
- SQL Server database with Entity Framework Core
- Input validation and error handling
- Structured logging with Serilog
- Docker containerization
- Swagger API documentation
- Comprehensive unit tests (22 tests)

## Technologies

- .NET 8.0
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- JWT Authentication
- BCrypt password hashing
- Serilog logging
- Docker & Docker Compose
- xUnit, Moq, FluentAssertions (Testing)

## Prerequisites

- Docker and Docker Compose (recommended)
- .NET 8.0 SDK and SQL Server

## Getting Started

### Using Docker (Recommended)

1. Clone the repository

2. Create a `.env` file from the example template:

```bash
cp .env.example .env
```

3. Update the `.env` file with your own secure values:
   - `SQL_SA_PASSWORD` - Strong password for SQL Server
   - `JWT_SECRET_KEY` - Base64-encoded secret key (generate with: `openssl rand -base64 32`)

4. Run the application with Docker Compose:

```bash
docker-compose up --build
```

The API will be available at `http://localhost:8080`

### Manual Setup

1. Install .NET 8.0 SDK
2. Configure SQL Server connection in `appsettings.json`
3. Configure JWT settings in `appsettings.json` or user secrets
4. Run migrations:

```bash
dotnet ef database update
```

5. Run the application:

```bash
dotnet run
```

## API Documentation

Once running, access the Swagger UI at:
- `http://localhost:8080/swagger` (Docker)
- `https://localhost:7XXX/swagger` (Local development)

## Configuration

### Environment Variables

Docker configuration uses environment variables from the `.env` file:

- `SQL_SA_PASSWORD` - SQL Server SA password (i wouldn't do this for a real production environments, dotnet secrets exist for a reason)
- `JWT_SECRET_KEY` - Secret key for JWT token generation
- `JWT_ISSUER` - Token issuer
- `JWT_AUDIENCE` - Token audience
- `JWT_EXPIRATION_MINUTES` - Token expiration time
- `DB_NAME` - Database name

### Application Settings

For manual setup, configure `appsettings.json`:

- `ConnectionStrings:DefaultConnection` - SQL Server connection string
- `Jwt:SecretKey` - Secret key for JWT token generation
- `Jwt:Issuer` - Token issuer
- `Jwt:Audience` - Token audience
- `Jwt:ExpirationInMinutes` - Token expiration time

## Security Best Practices

- Never commit `.env` files to version control (already in `.gitignore`)
- Use strong, unique passwords for production environments
- Generate secure JWT secret keys using: `openssl rand -base64 32`
- In production, use proper secrets management (Azure Key Vault, AWS Secrets Manager, etc.)
- Rotate secrets regularly

## Running Tests

The project includes comprehensive unit tests for services and controllers.

### Run all tests:

```bash
dotnet test
```

### Run tests with detailed output:

```bash
dotnet test --verbosity normal
```

### Test Coverage

The test suite includes:
- **AuthService Tests** (8 tests)
  - User registration with validation
  - User login with credential verification
  - JWT token generation and claims
  - Password hashing and verification
  - Duplicate email handling

- **TasksController Tests** (14 tests)
  - CRUD operations for tasks
  - User isolation (users can only access their own tasks)
  - Authorization checks
  - Task status management
  - Input validation

Total: 22 passing tests

## Project Structure

- `Controllers/` - API endpoints
- `Models/` - Database entities
- `DTOs/` - Data transfer objects
- `Data/` - Database context
- `Services/` - Business logic
- `Middleware/` - Custom middleware (error handling)
- `Migrations/` - EF Core migrations
- `TaskApi.Tests/` - Unit and integration tests

## Logging

Application logs are written to:
- Console output
- `logs/log-[date].txt` files

## License

This project is for educational purposes.

