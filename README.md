# Microservices Demo - .NET 10.0

Enterprise-grade microservices architecture implementing **Order**, **Product**, and **Payment** services with **Saga Pattern**, **Circuit Breaker**, and **API Gateway**.

## ??? Architecture Overview

```
                                    ???????????????????
                                    ?   API Gateway   ?
                                    ?  (Ocelot)       ?
                                    ?  Port: 7100     ?
                                    ???????????????????
                                             ?
                   ?????????????????????????????????????????????????????
                   ?                         ?                         ?
         ????????????????????     ????????????????????     ????????????????????
         ? Product Service  ?     ?  Order Service   ?     ? Payment Service  ?
         ?   Port: 7001     ?     ?   Port: 7000     ?     ?   Port: 7002     ?
         ?                  ??????? (Saga Orchestr.) ???????                  ?
         ????????????????????     ????????????????????     ????????????????????
                 ?                         ?                         ?
         ??????????????????       ??????????????????       ??????????????????
         ?   ProductDb    ?       ?    OrderDb     ?       ?   PaymentDb    ?
         ??????????????????       ??????????????????       ??????????????????
```

## ?? Features Implemented

### Core Features
? **Microservices Architecture** - Separate services with independent databases  
? **API Gateway** - Centralized entry point with Ocelot  
? **Saga Pattern** - Orchestration-based distributed transaction management  
? **Circuit Breaker** - Polly for resilience and fault tolerance  
? **Global Exception Handler** - Centralized error handling in all services  
? **CRUD Operations** - Complete Create, Read, Update, Delete for all entities  
? **Service-to-Service Communication** - HTTP-based REST APIs  
? **Compensation Transactions** - Automatic rollback on failures  
? **Database per Service** - Independent SQL Server databases  

### Design Patterns
- **Saga Orchestrator** - Order Service manages the workflow
- **Circuit Breaker** - 3 retries with exponential backoff
- **Repository Pattern** - EF Core DbContext
- **Dependency Injection** - Built-in ASP.NET Core DI
- **API Response Wrapper** - Consistent response format

### Technology Stack
- **.NET 10.0** - Latest framework
- **Entity Framework Core 9.0** - ORM
- **SQL Server (LocalDB)** - Database
- **Ocelot 23.4** - API Gateway
- **Polly** - Resilience and transient fault handling
- **Serilog** - Structured logging
- **Swagger/OpenAPI** - API documentation
- **NUnit** - Unit testing framework

## ?? Solution Structure

```
MicroservicesDemo/
??? src/
?   ??? ApiGateway/                     # API Gateway with Ocelot
?   ?   ??? ocelot.json                 # Gateway routing configuration
?   ?   ??? Program.cs
?   ?   ??? appsettings.json
?   ?
?   ??? Services/
?   ?   ??? OrderService/               # Order management & Saga orchestrator
?   ?   ?   ??? Controllers/
?   ?   ?   ?   ??? OrdersController.cs
?   ?   ?   ??? Data/
?   ?   ?   ?   ??? OrderDbContext.cs
?   ?   ?   ??? Models/
?   ?   ?   ?   ??? Order.cs
?   ?   ?   ??? Services/
?   ?   ?   ?   ??? IOrderSagaOrchestrator.cs
?   ?   ?   ?   ??? OrderSagaOrchestrator.cs
?   ?   ?   ??? Middleware/
?   ?   ?   ?   ??? GlobalExceptionHandler.cs
?   ?   ?   ??? Program.cs
?   ?   ?
?   ?   ??? ProductService/             # Product catalog & inventory
?   ?   ?   ??? Controllers/
?   ?   ?   ?   ??? ProductsController.cs
?   ?   ?   ??? Data/
?   ?   ?   ?   ??? ProductDbContext.cs
?   ?   ?   ??? Models/
?   ?   ?   ?   ??? Product.cs
?   ?   ?   ??? Middleware/
?   ?   ?   ?   ??? GlobalExceptionHandler.cs
?   ?   ?   ??? Program.cs
?   ?   ?
?   ?   ??? PaymentService/             # Payment processing
?   ?       ??? Controllers/
?   ?       ?   ??? PaymentsController.cs
?   ?       ??? Data/
?   ?       ?   ??? PaymentDbContext.cs
?   ?       ??? Models/
?   ?       ?   ??? Payment.cs
?   ?       ??? Middleware/
?   ?       ?   ??? GlobalExceptionHandler.cs
?   ?       ??? Program.cs
?   ?
?   ??? BuildingBlocks/
?       ??? Shared/                     # Shared DTOs, Models, Exceptions
?           ??? DTOs/
?           ?   ??? ProductDto.cs
?           ?   ??? OrderDto.cs
?           ?   ??? PaymentDto.cs
?           ??? Models/
?           ?   ??? ApiResponse.cs
?           ??? Exceptions/
?           ?   ??? CustomExceptions.cs
?           ??? Constants/
?               ??? Constants.cs
?
??? tests/
?   ??? ProductService.Tests/
?   ??? OrderService.Tests/
?   ??? PaymentService.Tests/
?   ??? IntegrationTests/
?
??? docs/
    ??? API_DOCUMENTATION.md
```

## ?? Getting Started

### Prerequisites
- .NET 10.0 SDK
- SQL Server (LocalDB)
- Visual Studio 2025 or VS Code
- Postman (optional, for API testing)

### Installation & Setup

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd MicroservicesDemo
   ```

2. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

3. **Apply database migrations**
   ```bash
   # Product Service
   cd src/Services/ProductService
   dotnet ef migrations add InitialCreate
   dotnet ef database update

   # Order Service
   cd ../OrderService
   dotnet ef migrations add InitialCreate
   dotnet ef database update

   # Payment Service
   cd ../PaymentService
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

4. **Run all services**
   
   Open **4 separate terminals** and run:

   ```bash
   # Terminal 1 - Product Service
   cd src/Services/ProductService
   dotnet run

   # Terminal 2 - Order Service
   cd src/Services/OrderService
   dotnet run

   # Terminal 3 - Payment Service
   cd src/Services/PaymentService
   dotnet run

   # Terminal 4 - API Gateway
   cd src/ApiGateway
   dotnet run
   ```

5. **Verify services are running**
   - Product Service: https://localhost:7001/swagger
   - Order Service: https://localhost:7000/swagger
   - Payment Service: https://localhost:7002/swagger
   - API Gateway: https://localhost:7100

## ?? Testing the Saga Flow

### Using API Gateway

**Step 1: Get available products**
```bash
curl -X GET https://localhost:7100/gateway/products
```

**Step 2: Create an order (triggers Saga)**
```bash
curl -X POST https://localhost:7100/gateway/orders \
  -H "Content-Type: application/json" \
  -d '{
    "productId": 1,
    "quantity": 2
  }'
```

**Expected Flow:**
```
User Request
    ?
[1] Create Order (Order Service) ? Status: Pending
    ?
[2] Update Inventory (Product Service) ? Status: Ordered
    ?
[3] Process Payment (Payment Service) ? Status: PaymentProcessed
    ?
[4] Confirm Order (Order Service) ? Status: Completed
    ?
? Order Complete
```

**If any step fails (e.g., insufficient inventory or payment declined):**
```
Failure Detected
    ?
[Compensation] Rollback Inventory
    ?
[Compensation] Mark Order as Failed
    ?
? Order Failed
```

### Using Direct Service Endpoints

**Create Product:**
```bash
curl -X POST https://localhost:7001/api/products \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Test Product",
    "description": "A test product",
    "price": 99.99,
    "quantity": 100,
    "category": "Electronics"
  }'
```

**Get All Orders:**
```bash
curl -X GET https://localhost:7000/api/orders
```

**Get Payments by Order ID:**
```bash
curl -X GET https://localhost:7002/api/payments/order/1
```

## ?? Database Schema

### Product Table
```sql
CREATE TABLE Products (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000),
    Price DECIMAL(18,2) NOT NULL,
    Quantity INT NOT NULL,
    Category NVARCHAR(100),
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL
);

CREATE INDEX IDX_ProductName ON Products(Name);
CREATE INDEX IDX_ProductCategory ON Products(Category);
```

### Order Table
```sql
CREATE TABLE Orders (
    Id INT PRIMARY KEY IDENTITY(1,1),
    ProductId INT NOT NULL,
    Quantity INT NOT NULL,
    TotalAmount DECIMAL(18,2) NOT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    CompletedAt DATETIME2 NULL,
    FailureReason NVARCHAR(500) NULL
);

CREATE INDEX IDX_OrderStatus ON Orders(Status);
CREATE INDEX IDX_OrderCreatedAt ON Orders(CreatedAt);
```

### Payment Table
```sql
CREATE TABLE Payments (
    Id INT PRIMARY KEY IDENTITY(1,1),
    OrderId INT NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    PaymentMethod NVARCHAR(50) NOT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    TransactionId NVARCHAR(100) NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    ProcessedAt DATETIME2 NULL,
    FailureReason NVARCHAR(500) NULL
);

CREATE INDEX IDX_PaymentOrderId ON Payments(OrderId);
CREATE INDEX IDX_PaymentStatus ON Payments(Status);
```

## ?? Configuration

### Connection Strings
Update in `appsettings.json` for each service:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=<ServiceName>Db;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

### Service URLs
Update in `OrderService/appsettings.json`:

```json
{
  "Services": {
    "ProductService": "https://localhost:7001",
    "PaymentService": "https://localhost:7002"
  }
}
```

## ??? Circuit Breaker Configuration

The Order Service implements circuit breaker pattern using Polly:

- **Retry Policy**: 3 retries with exponential backoff (2^n seconds)
- **Circuit Breaker**: Opens after 5 consecutive failures, breaks for 30 seconds
- **Timeout**: 10 seconds per request

## ?? API Endpoints

### API Gateway (Port 7100)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/gateway/products` | Get all products |
| GET | `/gateway/products/{id}` | Get product by ID |
| POST | `/gateway/products` | Create product |
| PUT | `/gateway/products/{id}` | Update product |
| DELETE | `/gateway/products/{id}` | Delete product |
| GET | `/gateway/orders` | Get all orders |
| GET | `/gateway/orders/{id}` | Get order by ID |
| POST | `/gateway/orders` | Create order (Saga) |
| PUT | `/gateway/orders/{id}/status` | Update order status |
| DELETE | `/gateway/orders/{id}` | Delete order |
| GET | `/gateway/payments` | Get all payments |
| GET | `/gateway/payments/{id}` | Get payment by ID |
| GET | `/gateway/payments/order/{orderId}` | Get payments for order |

## ?? Running Tests

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/OrderService.Tests/OrderService.Tests.csproj

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run tests with verbose output
dotnet test --logger:"console;verbosity=detailed"
```

## ?? Monitoring & Logging

All services use **Serilog** for structured logging:
- Console output for real-time monitoring
- File logs in `logs/` directory (rolling daily)
- Request/response logging
- Exception logging with stack traces (dev only)

**Log Locations:**
- Product Service: `logs/product-service-*.txt`
- Order Service: `logs/order-service-*.txt`
- Payment Service: `logs/payment-service-*.txt`
- API Gateway: `logs/api-gateway-*.txt`

## ?? Error Handling

Global exception handler provides consistent error responses:

```json
{
  "success": false,
  "message": "Error description",
  "data": null,
  "statusCode": 400,
  "errors": ["Detailed error message"],
  "timestamp": "2026-02-13T10:30:00Z"
}
```

**Exception Types:**
- `NotFoundException` ? 404
- `BadRequestException` ? 400
- `InsufficientInventoryException` ? 400
- `PaymentFailedException` ? 402
- `ServiceCommunicationException` ? 503

## ?? Security Considerations

**For Production:**
- [ ] Implement authentication (JWT)
- [ ] Add authorization policies
- [ ] Enable HTTPS only
- [ ] Configure CORS properly
- [ ] Add rate limiting
- [ ] Implement API keys
- [ ] Add request validation
- [ ] Secure connection strings
- [ ] Enable data encryption

## ?? Additional Resources

- [Saga Pattern Documentation](docs/SAGA_PATTERN.md)
- [Circuit Breaker Guide](docs/CIRCUIT_BREAKER.md)
- [API Documentation](docs/API_DOCUMENTATION.md)
- [Deployment Guide](docs/DEPLOYMENT.md)

## ?? Contributing

1. Fork the repository
2. Create feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Open Pull Request

## ?? License

This project is licensed under the MIT License.

## ?? Authors

- **Your Name** - *Initial work*

## ?? Acknowledgments

- Microsoft .NET Team
- Ocelot API Gateway
- Polly Project
- Entity Framework Core Team
