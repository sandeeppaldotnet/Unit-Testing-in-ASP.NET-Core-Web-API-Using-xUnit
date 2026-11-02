# ProductHub

ProductHub is a small ASP.NET Core Minimal API for managing products and calculating product discounts. The application targets **.NET 10** and currently uses an in-memory repository, making it suitable for learning, prototyping, and automated testing without an external database.

## Features

- Health-check endpoint for verifying that the API is running.
- Retrieve a product by ID.
- Create a product with service-level validation.
- Calculate a discounted product price.
- OpenAPI document generation in the Development environment.
- Unit tests for product-service behavior using xUnit and Moq.
- Dependency injection through separate repository and service abstractions.

## Technology Stack

- .NET 10
- ASP.NET Core Minimal APIs
- C# with nullable reference types enabled
- `Microsoft.AspNetCore.OpenApi` 10.0.8
- xUnit 2.9.3
- Moq 4.21.0
- Coverlet for test coverage collection

## Repository Structure

```text
ProductHub/
├── ProductHub.API/
│   ├── Models/
│   │   └── Product.cs
│   ├── Repositories/
│   │   ├── IProductRepository.cs
│   │   └── InMemoryProductRepository.cs
│   ├── Services/
│   │   ├── IProductService.cs
│   │   └── ProductService.cs
│   ├── Program.cs
│   ├── ProductHub.API.csproj
│   ├── ProductHub.API.http
│   ├── appsettings.json
│   └── Properties/
│       └── launchSettings.json
├── ProductHub.Tests/
│   ├── Services/
│   │   └── ProductServiceTests.cs
│   └── ProductHub.Tests.csproj
└── README.md
```

### Application layers

- **Models**: Defines the `Product` data model.
- **Repositories**: Provides product persistence abstractions. The current implementation stores products in a process-local list.
- **Services**: Contains business rules for retrieving products, creating products, and calculating discounts.
- **API**: Maps HTTP requests to service operations and converts validation failures into HTTP 400 responses.
- **Tests**: Verifies the service independently from persistence by using a mocked repository.

## Prerequisites

Install the following before running the project:

1. [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
2. Git, if cloning the repository
3. An HTTP client such as `curl`, PowerShell, Visual Studio, or VS Code REST Client

Verify the SDK installation:

```powershell
dotnet --version
```

The returned version should be a .NET 10 SDK version.

## Getting Started

From the repository root, restore dependencies and build the solution:

```powershell
dotnet restore
dotnet build
```

Start the API with the HTTP profile:

```powershell
dotnet run --project .\ProductHub.API\ProductHub.API.csproj --launch-profile http
```

The HTTP development profile listens on:

```text
http://localhost:5274
```

The HTTPS development profile listens on:

```text
https://localhost:7166
```

To start with the HTTPS profile:

```powershell
dotnet run --project .\ProductHub.API\ProductHub.API.csproj --launch-profile https
```

The HTTPS development certificate may need to be trusted on a local machine. If required, run:

```powershell
dotnet dev-certs https --trust
```

## Configuration

The API uses the standard ASP.NET Core configuration files:

- `ProductHub.API/appsettings.json`
- `ProductHub.API/appsettings.Development.json`
- Environment variables and command-line arguments

The default logging configuration sets the application log level to `Information` and ASP.NET Core framework logs to `Warning`.

No database connection string or external service configuration is currently required.

## API Reference

All product endpoints use the `/api` prefix.

### Health check

Returns the current application status.

```http
GET /api/health
```

Example response:

```json
{
  "status": "Healthy",
  "message": "ProductHub API is running."
}
```

### Get a product

Retrieves a product by its numeric ID.

```http
GET /api/products/{id}
```

Example request:

```powershell
Invoke-RestMethod -Method Get -Uri "http://localhost:5274/api/products/1"
```

Example response:

```json
{
  "id": 1,
  "name": "Mechanical Keyboard",
  "price": 2500,
  "stock": 20
}
```

Responses:

- `200 OK`: Product found.
- `400 Bad Request`: The ID is zero or negative.
- `404 Not Found`: No product exists with the requested ID.

The in-memory repository is initialized with these products:

| ID | Name | Price | Stock |
|---:|---|---:|---:|
| 1 | Mechanical Keyboard | 2500 | 20 |
| 2 | Wireless Mouse | 1200 | 35 |

### Create a product

Creates a product in the in-memory repository.

```http
POST /api/products
Content-Type: application/json
```

Example request:

```powershell
$body = @{
	name = "Gaming Headset"
	price = 3500
	stock = 15
} | ConvertTo-Json

Invoke-RestMethod `
	-Method Post `
	-Uri "http://localhost:5274/api/products" `
	-ContentType "application/json" `
	-Body $body
```

Example response:

```json
{
  "id": 3,
  "name": "Gaming Headset",
  "price": 3500,
  "stock": 15
}
```

Responses:

- `201 Created`: Product created successfully. The `Location` header points to the new product.
- `400 Bad Request`: The product fails validation.

Validation rules:

- `name` must not be null, empty, or whitespace.
- `price` must be greater than zero.
- `stock` must be zero or greater.

Example validation response:

```json
{
  "message": "Product price must be greater than zero."
}
```

### Calculate a discount

Calculates the final price without creating or changing a product.

```http
GET /api/products/discount?price={price}&discount={discountPercentage}
```

The discount is expressed as a percentage from `0` through `100`.

Example request:

```powershell
Invoke-RestMethod `
	-Method Get `
	-Uri "http://localhost:5274/api/products/discount?price=1000&discount=10"
```

Example response:

```json
{
  "originalPrice": 1000,
  "discountPercentage": 10,
  "finalPrice": 900
}
```

Responses:

- `200 OK`: Discount calculated successfully.
- `400 Bad Request`: `price` is not greater than zero, or the discount is outside the range `0` through `100`.

The calculation is:

```text
final price = price - (price × discount percentage ÷ 100)
```

## Testing

Run all tests from the repository root:

```powershell
dotnet test
```

Run tests with code coverage collection:

```powershell
dotnet test --collect:"XPlat Code Coverage"
```

The test suite covers:

- Valid and invalid discount calculations.
- Product IDs that are invalid, found, or not found.
- Successful product creation.
- Empty product names.
- Invalid prices.
- Negative stock values.
- Repository interaction verification for valid and invalid requests.

## OpenAPI

OpenAPI mapping is enabled in the Development environment. When the API is running with `ASPNETCORE_ENVIRONMENT=Development`, the generated document is available at:

```text
http://localhost:5274/openapi/v1.json
```

The project currently maps the OpenAPI document but does not include a Swagger UI package.

## Dependency Injection

The application registers its dependencies in `ProductHub.API/Program.cs`:

- `IProductRepository` uses a singleton `InMemoryProductRepository`.
- `IProductService` uses a scoped `ProductService`.

The service depends on `IProductRepository`, allowing the business logic to be tested without relying on the concrete in-memory implementation.

## Data Persistence and Limitations

The current repository is intentionally in-memory:

- Data is lost when the application stops or restarts.
- The repository is process-local and not shared between application instances.
- IDs are assigned using the current list count plus one.
- There are no update or delete endpoints.
- There is no product-list endpoint.
- There is no authentication or authorization.
- API-level request DTOs and advanced model validation have not been added.

For production use, replace `InMemoryProductRepository` with a database-backed implementation of `IProductRepository`, and add durable ID generation, persistence error handling, API versioning, authentication, authorization, and observability as needed.

## Development Workflow

A typical local workflow is:

```powershell
dotnet restore
dotnet build
dotnet test
dotnet run --project .\ProductHub.API\ProductHub.API.csproj --launch-profile http
```

You can also use `ProductHub.API/ProductHub.API.http` as a starting point for HTTP requests in Visual Studio or another compatible REST client. Update its request path if necessary so that it targets the ProductHub endpoints documented above.

## License

No license has been specified for this repository. Add a license file and update this section before distributing the project.
