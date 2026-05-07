# Bookstore API

A RESTful API for managing a bookstore, providing a complete CRUD API (and more) for books, authors, genres and reviews.

## Solution Overview

This solution is built with .NET 9 and follows Clean Architecture principles:

| Project | Responsibility |
|---------|---------------|
| `Bookstore.API` | Controllers, Swagger (OpenAPI), middleware |
| `Bookstore.Application` | Services, DTOs, interfaces, business logic |
| `Bookstore.Domain` | Entities |
| `Bookstore.Infrastructure` | EF Core, database context, migrations, Identity seeding |
| `Bookstore.Tests.Unit` | Unit tests (xUnit, in-memory database) |
| `Bookstore.Tests.Integration` | Integration tests (xUnit, SQL Server) |
| `Bookstore.Tests.Architecture` | Architecture tests (NetArchTest.Rules, Clean Architecture enforcement) |

### Key Features

- **Authentication & Authorization** — JWT tokens with ASP.NET Core Identity (role-based), with **refresh token support** for secure session renewal
- **Swagger Documentation** — comprehensive API documentation with example requests/responses
- **API Versioning** — URL-segment versioning (`/api/v1/...`, `/api/v2/...`)
- **Rate Limiting** — global fixed-window (60 req/min per IP) + per-user sliding-window (100 req/min per user) for authenticated endpoints
- **OData** — query books with `$filter`, `$orderby`, `$select`, `$expand`, `$top`, `$skip`, `$count` (v2)
- **Book Search** — stored-procedure-backed search with paging (v2)
- **Paging** — paginated responses for book list endpoints
- **Scheduled Import** — Quartz job imports books from a simulated third-party API every hour
- **Structured Logging** — Serilog with console, debug, and file sinks
- **Automatic Migrations** — EF Core migrations are applied on startup
- **Unit, Integration & Architecture Tests** — high test coverage for services, API endpoints, and Clean Architecture enforcement
- **Error Handling** — global exception handling with consistent error responses in the [ProblemDetails](https://datatracker.ietf.org/doc/html/rfc7807) standard format
- **Security Best Practices** — secure password policies, JWT signing, HTTPS enforcement and CORS

## How to Test the API Using Swagger

### Prerequisites

1. Visual Studio 2022+ 
2. SQL Server 2022+ instance
3. .NET 9+ SDK

### Getting Started

1. Clone the repository

2. **Configure User Secrets**
   - The connection string and JWT signing key are stored in User Secrets (not in source control).
   - The values below are **examples only** — you should change them to match your environment!
   - Run the following commands from the repository root:
   ```sh
   cd Bookstore.API
   dotnet user-secrets set "ConnectionStrings:BookstoreDB" "Data Source=.;Initial Catalog=Bookstore;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Application Name=Bookstore.API;Command Timeout=30"
   dotnet user-secrets set "JwtSettings:Secret" "MySuperSecretKey.ThatIsVery.VeryLong#123456#"
   ```          
   Adjust the connection string to match your SQL Server instance. The JWT secret can be any string ≥ 32 characters.

3. **Start the application**
    - Via CLI: `dotnet run --project Bookstore.API`
    - Via Visual Studio: select Debug configuration, Bookstore.API project, https profile and hit F5
    - Database migrations will be applied automatically on startup
    - In Development, default users and roles are seeded automatically

4. Navigate to Swagger UI
    - Default is `https://localhost:7146/swagger`, should start automatically

### Register a New User or Use Seeded Users

You have two options for obtaining user accounts:

#### Option 1: Register a New User via API

1. Expand the `POST /api/v1/Auth/register` endpoint in Swagger UI.
2. Click **Try it out**.
3. Enter a username (min 3 chars) and password (min 8 chars, must include uppercase, lowercase, digit, and special character):
   ```json
   {
     "username": "myuser",
     "password": "MyPassword1!"
   }
   ```
4. Click **Execute** — a `201 Created` response confirms registration.
5. New users are assigned the **Read** role by default.

#### Option 2: Use Pre-Seeded Users (Development Only)

When running in the development environment, the following users are automatically created:

- **reader** (role: `Read`)
  - Username: `reader`
  - Password: `Reader123!`
- **admin** (role: `ReadWrite`)
  - Username: `admin`
  - Password: `Admin123!`

You can use these accounts to log in and test the API immediately, without registering a new user.

### Testing with Read-Only User

1. **Get Authentication Token**
    - Expand the `POST /api/v1/Auth/login` endpoint
    - Click **Try it out**
    - Enter the following credentials:
      ```json
      {
        "username": "reader",
        "password": "Reader123!"
      }
      ```
    - Click **Execute**
    - Copy the `accessToken` value from the response

2. **Authorize in Swagger**
    - Click the **Authorize** button (green lock icon at the top right)
    - In the **Value** field, paste your token (without any prefix)
    - Click **Authorize**
    - Click **Close**

3. **Test Read Access**
    - You can now test all GET endpoints (Books, Authors, Genres, Reviews)
    - Example: Try `GET /api/v1/Books` to retrieve all books (paginated)
    - Try `GET /api/v1/Books/top-10` to get the top-rated books

4. **Verify Read-Only Restrictions**
    - Try any POST, PUT, or DELETE endpoint
    - You will receive a `403 Forbidden` response (insufficient permissions)

### Testing with Admin User (Full Access)

1. **Get Authentication Token**
    - If you are still logged in with previous user
        - Click the **Authorize** button (green lock icon at the top right)
        - Click **Logout**
    - Expand the `POST /api/v1/Auth/login` endpoint
    - Click **Try it out**
    - Enter the following credentials:
      ```json
      {
        "username": "admin",
        "password": "Admin123!"
      }
      ```
    - Click **Execute**
    - Copy the `accessToken` value from the response

2. **Authorize in Swagger**
    - Click the **Authorize** button
    - In the **Value** field, paste your token (without any prefix)
    - Click **Authorize**
    - Click **Close**

3. **Test Full Access**
    - You can now test all endpoints (GET, POST, PUT, DELETE)
    - Example: Create a new book using `POST /api/v1/Books`
    - Update a book's price using `PUT /api/v1/Books/{id}`
    - Delete a book using `DELETE /api/v1/Books/{id}`

## API Usage & Configuration


### Token Expiration & Refresh Token Flow

- **Access tokens** expire after **10 minutes** by default (configurable via `JwtSettings:AccessTokenExpirationMinutes` in `appsettings.Development.json`).
- **Refresh tokens** expire after **1 day** by default (configurable via `JwtSettings:RefreshTokenExpirationDays` in `appsettings.Development.json`).
- When an access token expires, you will receive `401 Unauthorized` responses.
- To renew your JWT access token without re-authenticating, use the `POST /api/v1/Auth/refresh` endpoint with your current access and refresh tokens:
  ```json
  {
    "accessToken": "<your-access-token>",
    "refreshToken": "<your-refresh-token>"
  }
  ```
- The server verifies the refresh token is valid, unexpired, and not revoked:
    - If valid, a new access/refresh token pair is returned.
    - If the refresh token is expired or invalid, you must log in again.

### Rate Limiting

- **Global**: 60 requests per minute per IP address (fixed window)
- **Authenticated endpoints**: 100 requests per minute per user (sliding window)
- Exceeding the limit returns `429 Too Many Requests` with a `Retry-After` header

### Manually Triggering the Scheduled Import Job

The API includes a scheduled job that imports books from a simulated third-party source every hour.
You can manually trigger this import:

1. **Authenticate as Admin**
2. Expand the `POST /api/v1/Import/trigger` endpoint
3. Click **Try it out**
4. Click **Execute**
5. The response will show the number of books imported
6. **Note**: The import process may take some time (usually around 1 minute) as it processes 100,000 books from the simulated API

### OData Queries (v2)

After authenticating, you can use the OData endpoint for flexible querying:

```
GET /api/v2/Books/odata?$filter=price lt 100&$orderby=title desc&$top=10&$skip=0&$count=true&$select=id,title,price&$expand=authors($select=name)
```

### Book Search (v2)

Search books by title, author, or genre with paging:

```
GET /api/v2/Books/search?title=...&author=...&genre=...&page=1&pageSize=10
```

## Technologies

- **.NET 9**
- **ASP.NET Core Web API**
- **ASP.NET Core Identity**
- **Entity Framework Core 9**
- **SQL Server**
- **JWT Authentication**
- **Swagger/OpenAPI**
- **Serilog**
- **Quartz.NET**
- **xUnit, Moq, FluentAssertions, NetArchTest.Rules**
