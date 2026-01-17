# Clean Architecture API

A comprehensive .NET 8 Web API demonstrating Clean Architecture principles with industry best practices.

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                         API Layer                                │
│  (Controllers, Middleware, Filters, Swagger Configuration)      │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                      Application Layer                           │
│  (Services, DTOs, Validators, AutoMapper Profiles, Interfaces)  │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                       Domain Layer                               │
│  (Entities, Domain Interfaces, Custom Exceptions, Value Objects)│
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                    Infrastructure Layer                          │
│  (Repositories, DbContext, External Service Implementations)    │
└─────────────────────────────────────────────────────────────────┘
```

## Features

### Core Features
- **Clean Architecture** - Controller → Service → Repository pattern
- **DTOs + AutoMapper** - Automatic object mapping between layers
- **Global Exception Handling** - Centralized error handling middleware
- **Structured Logging** - Serilog with console and file sinks
- **FluentValidation** - Request validation with detailed error messages
- **Pagination & Filtering** - Built-in support for all list endpoints

### Bonus Features
- **Unit Tests** - xUnit tests with Moq and FluentAssertions
- **Swagger Customization** - Interactive API documentation with annotations
- **In-Memory Database** - Ready to run without external dependencies
- **SQL Server Support** - Production-ready database configuration

## Project Structure

```
dotnet-clean-architecture-api/
├── src/
│   ├── CleanArchitecture.Domain/
│   │   ├── Common/           # Base classes
│   │   ├── Entities/         # Domain entities
│   │   ├── Exceptions/       # Custom exceptions
│   │   └── Interfaces/       # Repository interfaces
│   │
│   ├── CleanArchitecture.Application/
│   │   ├── Common/           # Shared models (ApiResponse, PagedResult)
│   │   ├── DTOs/             # Data Transfer Objects
│   │   ├── Interfaces/       # Service interfaces
│   │   ├── Mappings/         # AutoMapper profiles
│   │   ├── Services/         # Business logic
│   │   └── Validators/       # FluentValidation validators
│   │
│   ├── CleanArchitecture.Infrastructure/
│   │   ├── Persistence/      # DbContext & configurations
│   │   └── Repositories/     # Repository implementations
│   │
│   └── CleanArchitecture.API/
│       ├── Controllers/      # API endpoints
│       ├── Middleware/       # Custom middleware
│       └── Filters/          # Action filters
│
└── tests/
    └── CleanArchitecture.UnitTests/
        ├── Controllers/      # Controller tests
        ├── Services/         # Service tests
        └── Validators/       # Validator tests
```

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- Visual Studio 2022 / VS Code / JetBrains Rider

### Running the Application

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd dotnet-clean-architecture-api
   ```

2. **Restore packages**
   ```bash
   dotnet restore
   ```

3. **Run the application**
   ```bash
   dotnet run --project src/CleanArchitecture.API
   ```

4. **Open Swagger UI**
   Navigate to `https://localhost:7001` or `http://localhost:5000`

### Running Tests

```bash
dotnet test
```

## API Endpoints

### Products API
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/products` | Get all products (paginated) |
| GET | `/api/products/{id}` | Get product by ID |
| GET | `/api/products/category/{category}` | Get products by category |
| POST | `/api/products` | Create a new product |
| PUT | `/api/products/{id}` | Update a product |
| DELETE | `/api/products/{id}` | Delete a product |

### Customers API
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/customers` | Get all customers (paginated) |
| GET | `/api/customers/{id}` | Get customer by ID |
| GET | `/api/customers/email/{email}` | Get customer by email |
| GET | `/api/customers/country/{country}` | Get customers by country |
| POST | `/api/customers` | Create a new customer |
| PUT | `/api/customers/{id}` | Update a customer |
| DELETE | `/api/customers/{id}` | Delete a customer |

## Query Parameters

### Pagination
- `pageNumber` - Page number (default: 1)
- `pageSize` - Items per page (default: 10, max: 100)

### Filtering (Products)
- `searchTerm` - Search in name and description
- `category` - Filter by category
- `minPrice` - Minimum price
- `maxPrice` - Maximum price
- `isActive` - Filter by active status
- `sortBy` - Sort field (name, price, category, createdAt)
- `sortDescending` - Sort direction (default: false)

### Filtering (Customers)
- `searchTerm` - Search in name and email
- `country` - Filter by country
- `city` - Filter by city
- `isActive` - Filter by active status
- `sortBy` - Sort field (firstName, lastName, email, country, createdAt)
- `sortDescending` - Sort direction (default: false)

## Configuration

### Database Connection
Update `appsettings.json` to use SQL Server:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=CleanArchitectureDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

Leave `DefaultConnection` empty to use In-Memory database (default for development).

## Technologies Used

- **.NET 8.0** - Framework
- **Entity Framework Core 8** - ORM
- **AutoMapper** - Object mapping
- **FluentValidation** - Request validation
- **Serilog** - Structured logging
- **Swashbuckle** - Swagger/OpenAPI documentation
- **xUnit** - Testing framework
- **Moq** - Mocking library
- **FluentAssertions** - Test assertions

## License

This project is licensed under the MIT License.
