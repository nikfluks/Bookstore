# Bookstore API

A RESTful API for managing a bookstore, providing a complete CRUD API for books, authors, genres and reviews. 

## Solution Overview

This solution is built with .NET 9 and follows Clean Architecture principles.
It provides JWT-based authentication and authorization as well as comprehensive logging.
There is also scheduled data import functionality and unit and integrations tests.

## How to Test the API Using Swagger

### Prerequisites

1. Visual Studio 2022
2. SQL Server instance 

### Getting Started

1. Clone the repository
2. Update the connection string in `appsettings.Development.json` if necessary
3. Start the application: `dotnet run --project Bookstore.API` or in VS: select Debug configuration with https profile and F5
    - Database migrations will be applied automatically on startup                
4. Navigate to Swagger UI 
    - Default is `https://localhost:7146/swagger`, should start automatically

### Testing with Read-Only User

1. **Get Authentication Token**
    - Expand the `/api/Auth/login` endpoint
    - Click **Try it out**
    - Enter the following credentials:
     `{
         "username": "reader",
         "password": "reader123"
      }`
   - Click **Execute**
   - Copy the `token` value from the response

2. **Authorize in Swagger**
    - Click the **Authorize** button (green lock icon at the top right)
    - In the **Value** field, paste your token (without any prefix)
    - Click **Authorize**
    - Click **Close**

3. **Test Read Access**
    - You can now test all GET endpoints (Books, Authors, Genres, Reviews)
    - Example: Try `/api/Books` to retrieve all books
    - Try `/api/Books/top-10` to get the top-rated books

4. **Verify Read-Only Restrictions**
    - Try any POST, PUT, or DELETE endpoint
    - You will receive a `403 Forbidden` response (insufficient permissions)

### Testing with Admin User (Full Access)

1. **Get Authentication Token**
    - If you are still logged in with previous user
        - Click the **Authorize** button (green lock icon at the top right)
        - Click **Logout**
    - Expand the `/api/Auth/login` endpoint
    - Click **Try it out**
    - Enter the following credentials:
     `{
         "username": "admin",
         "password": "admin123"
      }`
    - Click **Execute**
    - Copy the `token` value from the response

2. **Authorize in Swagger**
    - Click the **Authorize** button
    - In the **Value** field, paste your token (without any prefix)
    - Click **Authorize**
    - Click **Close**

3. **Test Full Access**
   - You can now test all endpoints (GET, POST, PUT, DELETE)
   - Example: Create a new book using `/api/Books` POST endpoint
   - Update a book's price using `/api/Books/{id}` PUT endpoint
   - Delete a book using `/api/Books/{id}` DELETE endpoint

### Manually Triggering the Scheduled Import Job

The API includes a scheduled job that imports books from a simulated third-party source every hour. 
You can manually trigger this import:

1. **Authenticate as Admin**
2. Expand the `/api/Import/trigger` endpoint
3. Click **Try it out**
4. Click **Execute**
5. The response will show the number of books imported
6. **Note**: The import process may take some time (usually around 1 minute) as it processes 100,000 books from the simulated API

### Token Expiration

- Tokens expire after **10 minutes** by default
- When a token expires, you will receive `401 Unauthorized` responses
- Simply repeat the login process to get a new token
- Token expiration is configurable via `appsettings.Development.json`

## Key Features

### 🔐 Authentication & Authorization (JWT)

- **JWT Bearer Token authentication** with role-based access control
- **Two roles**: 
  - `Read` - Access to all GET endpoints
  - `ReadWrite` - Full access to all endpoints
- **Default users**:
  - Reader: `reader/reader123` with `Read` role
  - Admin: `admin/admin123` with `ReadWrite` role

### 📊 Swagger UI

- Interactive API documentation with built-in authentication
- Try out endpoints directly from the browser
- Auto-generated from controller attributes

### 📝 Serilog Logging

- Structured logging with contextual information
- Console and file sinks with different log levels per environment
- Request/response logging middleware
- Enriched with machine name, thread ID, and environment in non-development

### 🗄️ Entity Framework Core with SQL Server

- Code-first approach with migrations
- Many-to-many relationships (Books ↔ Authors, Books ↔ Genres)
- Optimized queries with proper indexing

### ⏰ Scheduled Jobs (Quartz.NET)

- Automated book import job running every hour
- Manual trigger endpoint for on-demand imports (`/api/Import/trigger`)
- Concurrent execution prevention
- Imports 100,000+ books from simulated third-party API
- Automatic deduplication and relationship management

### ✅ Testing

- **Unit Tests**: In-memory database for fast, isolated testing
- **Integration Tests**: Real SQL Server database for end-to-end validation
- Comprehensive coverage for services and API endpoints
- Test fixtures for database setup and teardown

## Architecture

The solution is organized into the following projects:

- **Bookstore.API**: Web API layer with controllers, middleware, and Swagger configuration
- **Bookstore.Application**: Business logic layer with services and interfaces
- **Bookstore.Domain**: Core domain entities and models
- **Bookstore.Infrastructure**: Data access layer with EF Core and database context
- **Bookstore.Tests.Unit**: Unit tests with in-memory database
- **Bookstore.Tests.Integration**: Integration tests with SQL Server

## Technologies

- **.NET 9**
- **ASP.NET Core Web API**
- **Entity Framework Core 9**
- **SQL Server**
- **JWT Authentication**
- **Swagger/OpenAPI**
- **Serilog**
- **Quartz.NET**
- **xUnit, Moq, FluentAssertions**

## API Endpoints

### Authentication

- `POST /api/Auth/login` - Get JWT token

### Books

- `GET /api/Books` - Get all books with details
- `GET /api/Books/top-10` - Get top 10 books by rating
- `GET /api/Books/{id}` - Get book by ID
- `POST /api/Books` - Create new book (ReadWrite)
- `PUT /api/Books/{id}` - Update book price (ReadWrite)
- `PUT /api/Books/{id}/authors` - Update book authors (ReadWrite)
- `PUT /api/Books/{id}/genres` - Update book genres (ReadWrite)
- `DELETE /api/Books/{id}` - Delete book (ReadWrite)

### Authors

- `GET /api/Authors` - Get all authors
- `GET /api/Authors/{id}` - Get author by ID
- `POST /api/Authors` - Create new author (ReadWrite)
- `PUT /api/Authors/{id}` - Update author (ReadWrite)
- `DELETE /api/Authors/{id}` - Delete author (ReadWrite)

### Genres

- `GET /api/Genres` - Get all genres
- `GET /api/Genres/{id}` - Get genre by ID
- `POST /api/Genres` - Create new genre (ReadWrite)
- `PUT /api/Genres/{id}` - Update genre (ReadWrite)
- `DELETE /api/Genres/{id}` - Delete genre (ReadWrite)

### Reviews

- `GET /api/Reviews` - Get all reviews
- `GET /api/Reviews/{id}` - Get review by ID
- `POST /api/Reviews` - Create new review (ReadWrite)
- `PUT /api/Reviews/{id}` - Update review (ReadWrite)
- `DELETE /api/Reviews/{id}` - Delete review (ReadWrite)

### Import

- `POST /api/Import/trigger` - Manually trigger book import (ReadWrite)