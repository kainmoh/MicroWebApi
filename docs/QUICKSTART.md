# ?? Quick Start Guide

This guide will help you get the microservices solution up and running quickly.

## Prerequisites

- **.NET 10.0 SDK** - [Download](https://dotnet.microsoft.com/download)
- **SQL Server LocalDB** - Included with Visual Studio
- **PowerShell** or **Bash** terminal

## Quick Setup (5 minutes)

### Step 1: Restore Dependencies
```bash
dotnet restore
```

### Step 2: Build Solution
```bash
dotnet build
```

### Step 3: Create Databases
```bash
# Navigate to each service and create migrations

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

### Step 4: Run Services

**Option A: Using Visual Studio**
1. Open `MicroservicesDemo.slnx`
2. Right-click solution ? **Configure Startup Projects**
3. Select **Multiple startup projects**
4. Set all 4 services to **Start**:
   - ApiGateway
   - ProductService
   - OrderService
   - PaymentService
5. Press F5

**Option B: Using Command Line (4 separate terminals)**

Terminal 1 - Product Service:
```bash
cd src/Services/ProductService
dotnet run
```

Terminal 2 - Order Service:
```bash
cd src/Services/OrderService
dotnet run
```

Terminal 3 - Payment Service:
```bash
cd src/Services/PaymentService
dotnet run
```

Terminal 4 - API Gateway:
```bash
cd src/ApiGateway
dotnet run
```

### Step 5: Verify Setup

Open these URLs in your browser:

- ? API Gateway: https://localhost:7100
- ? Product Service Swagger: https://localhost:7001/swagger
- ? Order Service Swagger: https://localhost:7000/swagger
- ? Payment Service Swagger: https://localhost:7002/swagger

## ?? Test the Saga Flow

### Using Swagger UI (Recommended for first time)

1. Open **Product Service** Swagger: https://localhost:7001/swagger
2. GET `/api/products` - View available products
3. Open **Order Service** Swagger: https://localhost:7000/swagger
4. POST `/api/orders` - Create order with this JSON:
```json
{
  "productId": 1,
  "quantity": 2
}
```
5. Watch the console logs to see the Saga flow
6. GET `/api/orders/{id}` - Check order status
7. Open **Payment Service** Swagger: https://localhost:7002/swagger
8. GET `/api/payments/order/{orderId}` - View payment details

### Using cURL

Create an order via API Gateway:
```bash
curl -X POST https://localhost:7100/gateway/orders \
  -H "Content-Type: application/json" \
  -d "{\"productId\": 1, \"quantity\": 2}"
```

View all orders:
```bash
curl https://localhost:7100/gateway/orders
```

### Using Postman

Import this collection:

```json
{
  "info": { "name": "Microservices Demo" },
  "item": [
    {
      "name": "Get Products",
      "request": {
        "method": "GET",
        "url": "https://localhost:7100/gateway/products"
      }
    },
    {
      "name": "Create Order",
      "request": {
        "method": "POST",
        "url": "https://localhost:7100/gateway/orders",
        "header": [{"key": "Content-Type", "value": "application/json"}],
        "body": {
          "mode": "raw",
          "raw": "{\n  \"productId\": 1,\n  \"quantity\": 2\n}"
        }
      }
    }
  ]
}
```

## ?? What Happens During Order Creation

### Successful Flow:
```
1. Order created with status "Pending"
2. Product inventory reduced (quantity - 2)
3. Order status ? "Ordered"
4. Payment processed (95% success rate)
5. Order status ? "PaymentProcessed"
6. Order status ? "Completed"
? Order complete!
```

### Failure with Rollback:
```
1. Order created with status "Pending"
2. Product inventory reduced
3. Order status ? "Ordered"
4. Payment FAILS (5% chance)
5. ?? COMPENSATION: Inventory restored (quantity + 2)
6. Order status ? "Failed"
? Order failed, inventory restored
```

## ?? View Logs

All services log to console and files:

```bash
# View Product Service logs
cat src/Services/ProductService/logs/product-service-*.txt

# View Order Service logs (includes Saga flow)
cat src/Services/OrderService/logs/order-service-*.txt

# View Payment Service logs
cat src/Services/PaymentService/logs/payment-service-*.txt
```

## ?? Troubleshooting

### Port Already in Use
```bash
# Change ports in appsettings.json:
"Urls": "https://localhost:7001"  # Change to 7011, 7021, etc.
```

### Database Connection Error
```bash
# Verify LocalDB is running:
sqllocaldb info
sqllocaldb start MSSQLLocalDB

# Or change connection string to full SQL Server:
"Server=localhost;Database=ProductDb;Trusted_Connection=True;"
```

### Migration Errors
```bash
# Delete and recreate migrations:
cd src/Services/ProductService
dotnet ef database drop --force
dotnet ef migrations remove
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Service Communication Errors
- Ensure all 4 services are running
- Check firewall settings
- Verify URLs in `OrderService/appsettings.json`:
  ```json
  "Services": {
    "ProductService": "https://localhost:7001",
    "PaymentService": "https://localhost:7002"
  }
  ```

## ?? Next Steps

1. **Explore API Documentation**: Visit each service's Swagger UI
2. **Check Logs**: Watch console output during order creation
3. **Test Failure Scenarios**: 
   - Try ordering more quantity than available (should fail)
   - Payment service has 5% failure rate (retry multiple times)
4. **Run Tests**: `dotnet test`
5. **Read Architecture Guide**: See `docs/ARCHITECTURE.md`

## ?? Tips

- **Health Checks**: Each service exposes `/health` endpoint
- **Auto-Reload**: Changes auto-reload when using `dotnet watch run`
- **Debug Mode**: Set breakpoints in Visual Studio to step through Saga
- **Rate Limiting**: API Gateway limits requests (10/sec products, 5/sec orders)

## ?? Support

If you encounter issues:
1. Check the troubleshooting section above
2. Review logs in `logs/` directories
3. Verify all prerequisites are installed
4. Ensure correct .NET version: `dotnet --version`

Happy coding! ??
