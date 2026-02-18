# ?? Log Files Location Guide

## ?? Quick Answer

All log files are stored in the `logs/` directory within each service's project folder.

---

## ?? Log File Locations

### **1. Product Service Logs**
```
?? src/Services/ProductService/logs/
   ??? product-service-YYYYMMDD.txt
```

**Example:**
```
src/Services/ProductService/logs/product-service-20240215.txt
src/Services/ProductService/logs/product-service-20240216.txt
```

**Full Path:**
```
D:\react_proj\microproj\MicroWebApi\MicroWebApi\src\Services\ProductService\logs\
```

---

### **2. Order Service Logs**
```
?? src/Services/OrderService/logs/
   ??? order-service-YYYYMMDD.txt
```

**Example:**
```
src/Services/OrderService/logs/order-service-20240215.txt
src/Services/OrderService/logs/order-service-20240216.txt
```

**Full Path:**
```
D:\react_proj\microproj\MicroWebApi\MicroWebApi\src\Services\OrderService\logs\
```

---

### **3. Payment Service Logs**
```
?? src/Services/PaymentService/logs/
   ??? payment-service-YYYYMMDD.txt
```

**Example:**
```
src/Services/PaymentService/logs/payment-service-20240215.txt
src/Services/PaymentService/logs/payment-service-20240216.txt
```

**Full Path:**
```
D:\react_proj\microproj\MicroWebApi\MicroWebApi\src\Services\PaymentService\logs\
```

---

### **4. User Service Logs**
```
?? src/Services/UserService/logs/
   ??? user-service-YYYYMMDD.txt
```

**Example:**
```
src/Services/UserService/logs/user-service-20240215.txt
src/Services/UserService/logs/user-service-20240216.txt
```

**Full Path:**
```
D:\react_proj\microproj\MicroWebApi\MicroWebApi\src\Services\UserService\logs\
```

---

### **5. API Gateway Logs**
```
?? src/ApiGateway/logs/
   ??? api-gateway-YYYYMMDD.txt
```

**Example:**
```
src/ApiGateway/logs/api-gateway-20240215.txt
src/ApiGateway/logs/api-gateway-20240216.txt
```

**Full Path:**
```
D:\react_proj\microproj\MicroWebApi\MicroWebApi\src\ApiGateway\logs\
```

---

## ?? How to View Log Files

### **Option 1: Windows File Explorer**
1. Open File Explorer
2. Navigate to: `D:\react_proj\microproj\MicroWebApi\MicroWebApi\src\Services\[ServiceName]\logs\`
3. Double-click the log file to open in Notepad

### **Option 2: Visual Studio Code**
1. Open VS Code
2. Click on Explorer (Ctrl+Shift+E)
3. Navigate to `src/Services/[ServiceName]/logs/`
4. Click on the log file

### **Option 3: PowerShell**
```powershell
# View today's Product Service logs
Get-Content src/Services/ProductService/logs/product-service-*.txt -Tail 50

# View today's Order Service logs
Get-Content src/Services/OrderService/logs/order-service-*.txt -Tail 50

# View today's Payment Service logs
Get-Content src/Services/PaymentService/logs/payment-service-*.txt -Tail 50

# View today's User Service logs
Get-Content src/Services/UserService/logs/user-service-*.txt -Tail 50

# View today's API Gateway logs
Get-Content src/ApiGateway/logs/api-gateway-*.txt -Tail 50

# Watch logs in real-time (follow mode)
Get-Content src/Services/OrderService/logs/order-service-*.txt -Wait -Tail 20
```

### **Option 4: Command Prompt**
```cmd
# View last 50 lines of today's logs
type src\Services\ProductService\logs\product-service-20240215.txt | more

# Or use tail (if installed)
tail -f src\Services\ProductService\logs\product-service-20240215.txt
```

### **Option 5: Windows Terminal**
```bash
# Open specific log file
notepad src/Services/ProductService/logs/product-service-20240215.txt

# Or use PowerShell to tail
Get-Content src/Services/ProductService/logs/product-service-*.txt -Wait -Tail 50
```

---

## ?? Log File Format

### **Log Entry Structure**
```
[Timestamp] [LogLevel] [Context] Message
```

### **Example Log Entries**

#### **Information Log**
```
[2024-02-15 10:15:30.123] [Information] [ProductService.Controllers.ProductsController] Fetching all products
```

#### **Warning Log**
```
[2024-02-15 10:15:45.456] [Warning] [ProductService.Controllers.ProductsController] Product not found: 999
```

#### **Error Log**
```
[2024-02-15 10:16:00.789] [Error] [OrderService.Services.OrderSagaOrchestrator] Order saga failed: Payment processing failed
System.Exception: Payment processing failed
   at OrderService.Services.OrderSagaOrchestrator.ProcessPaymentAsync()
   at OrderService.Services.OrderSagaOrchestrator.ExecuteOrderSagaAsync()
```

#### **Circuit Breaker Log**
```
[2024-02-15 10:20:15.321] [Warning] Circuit breaker opened for 30s due to: Connection refused
```

#### **Retry Log**
```
[2024-02-15 10:25:30.654] [Warning] Retry 1 after 2s due to: The server is not responding
[2024-02-15 10:25:34.987] [Warning] Retry 2 after 4s due to: The server is not responding
[2024-02-15 10:25:42.123] [Warning] Retry 3 after 8s due to: The server is not responding
```

---

## ?? Finding Specific Errors

### **Search for Errors in PowerShell**

#### **1. Find all errors in Product Service**
```powershell
Select-String -Path "src/Services/ProductService/logs/*.txt" -Pattern "Error"
```

#### **2. Find specific error message**
```powershell
Select-String -Path "src/Services/OrderService/logs/*.txt" -Pattern "saga failed"
```

#### **3. Find circuit breaker events**
```powershell
Select-String -Path "src/Services/OrderService/logs/*.txt" -Pattern "Circuit breaker"
```

#### **4. Find all warnings and errors**
```powershell
Select-String -Path "src/Services/*/logs/*.txt" -Pattern "Warning|Error"
```

#### **5. Search across all services**
```powershell
Get-ChildItem -Path "src/Services/*/logs/*.txt" -Recurse | 
    Select-String -Pattern "Payment processing failed"
```

### **Search for Errors in Command Prompt**

```cmd
# Find errors in specific log file
findstr /i "error" src\Services\ProductService\logs\product-service-20240215.txt

# Find circuit breaker events
findstr /i "circuit" src\Services\OrderService\logs\order-service-20240215.txt

# Find all warnings
findstr /i "warning" src\Services\OrderService\logs\order-service-20240215.txt
```

---

## ?? Common Errors and Where to Find Them

### **1. Product Not Found (404)**
**Service:** Product Service  
**Log File:** `src/Services/ProductService/logs/product-service-*.txt`  
**Search For:**
```powershell
Select-String -Path "src/Services/ProductService/logs/*.txt" -Pattern "Product not found"
```

**Example Log:**
```
[2024-02-15 10:30:00] [Warning] [ProductService.Controllers.ProductsController] Product not found: 999
```

---

### **2. Insufficient Inventory**
**Service:** Product Service  
**Log File:** `src/Services/ProductService/logs/product-service-*.txt`  
**Search For:**
```powershell
Select-String -Path "src/Services/ProductService/logs/*.txt" -Pattern "Insufficient inventory"
```

**Example Log:**
```
[2024-02-15 10:35:00] [Warning] [ProductService.Controllers.ProductsController] Insufficient inventory for product: 1. Available: 10, Required: 50
```

---

### **3. Order Saga Failed**
**Service:** Order Service  
**Log File:** `src/Services/OrderService/logs/order-service-*.txt`  
**Search For:**
```powershell
Select-String -Path "src/Services/OrderService/logs/*.txt" -Pattern "saga failed"
```

**Example Log:**
```
[2024-02-15 10:40:00] [Error] [OrderService.Services.OrderSagaOrchestrator] Order saga failed: Failed to process payment
[2024-02-15 10:40:00] [Information] [OrderService.Services.OrderSagaOrchestrator] Compensating transaction: Rolling back inventory for product 1
```

---

### **4. Payment Failed**
**Service:** Payment Service  
**Log File:** `src/Services/PaymentService/logs/payment-service-*.txt`  
**Search For:**
```powershell
Select-String -Path "src/Services/PaymentService/logs/*.txt" -Pattern "Payment processing failed"
```

**Example Log:**
```
[2024-02-15 10:45:00] [Warning] [PaymentService.Controllers.PaymentsController] Payment processing failed: Card declined
```

---

### **5. Circuit Breaker Opened**
**Service:** Order Service  
**Log File:** `src/Services/OrderService/logs/order-service-*.txt`  
**Search For:**
```powershell
Select-String -Path "src/Services/OrderService/logs/*.txt" -Pattern "Circuit breaker opened"
```

**Example Log:**
```
[2024-02-15 10:50:00] [Warning] Circuit breaker opened for 30s due to: Connection refused
[2024-02-15 10:50:30] [Information] Circuit breaker is half-open
[2024-02-15 10:50:35] [Information] Circuit breaker reset
```

---

### **6. Database Connection Errors**
**Search Across All Services:**
```powershell
Select-String -Path "src/Services/*/logs/*.txt" -Pattern "MySQL|Connection|Database"
```

**Example Log:**
```
[2024-02-15 11:00:00] [Error] Failed to connect to MySQL server at 127.0.0.1:3306
MySql.Data.MySqlClient.MySqlException: Unable to connect to any of the specified MySQL hosts
```

---

### **7. Authentication Errors**
**Service:** User Service  
**Log File:** `src/Services/UserService/logs/user-service-*.txt`  
**Search For:**
```powershell
Select-String -Path "src/Services/UserService/logs/*.txt" -Pattern "Invalid credentials|Unauthorized"
```

**Example Log:**
```
[2024-02-15 11:05:00] [Warning] [UserService.Controllers.AuthController] Login failed: Invalid credentials
```

---

## ??? Useful PowerShell Scripts

### **Script 1: View All Service Logs**
```powershell
# Create: view-all-logs.ps1
Get-ChildItem -Path "src/Services/*/logs/*.txt", "src/ApiGateway/logs/*.txt" -Recurse | 
    ForEach-Object { 
        Write-Host "`n=== $($_.FullName) ===" -ForegroundColor Cyan
        Get-Content $_.FullName -Tail 10
    }
```

**Run:**
```powershell
.\view-all-logs.ps1
```

---

### **Script 2: Watch Order Service Logs Live**
```powershell
# Create: watch-order-logs.ps1
$logPath = "src/Services/OrderService/logs/order-service-*.txt"
Write-Host "Watching Order Service logs... Press Ctrl+C to exit" -ForegroundColor Green
Get-Content $logPath -Wait -Tail 20
```

**Run:**
```powershell
.\watch-order-logs.ps1
```

---

### **Script 3: Find All Errors Today**
```powershell
# Create: find-errors.ps1
$today = Get-Date -Format "yyyyMMdd"
$services = @("ProductService", "OrderService", "PaymentService", "UserService")

foreach ($service in $services) {
    $logPath = "src/Services/$service/logs/*-service-$today.txt"
    if (Test-Path $logPath) {
        Write-Host "`n=== Errors in $service ===" -ForegroundColor Red
        Select-String -Path $logPath -Pattern "Error|Exception" | Select-Object -First 5
    }
}
```

**Run:**
```powershell
.\find-errors.ps1
```

---

### **Script 4: Clean Old Logs**
```powershell
# Create: clean-old-logs.ps1
# Delete logs older than 7 days
$daysToKeep = 7
$cutoffDate = (Get-Date).AddDays(-$daysToKeep)

Get-ChildItem -Path "src/Services/*/logs/*.txt", "src/ApiGateway/logs/*.txt" -Recurse | 
    Where-Object { $_.LastWriteTime -lt $cutoffDate } | 
    Remove-Item -Force

Write-Host "Cleaned logs older than $daysToKeep days" -ForegroundColor Green
```

**Run:**
```powershell
.\clean-old-logs.ps1
```

---

## ?? Log Level Configuration

### **Current Log Levels** (in `appsettings.json`)

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.AspNetCore": "Warning",
        "Microsoft.EntityFrameworkCore": "Warning",
        "System.Net.Http.HttpClient": "Warning"
      }
    }
  }
}
```

### **Log Levels Explained**

| Level | Description | What's Logged |
|-------|-------------|---------------|
| **Verbose** | Most detailed | Everything including debugging info |
| **Debug** | Debugging info | Useful for development |
| **Information** | Normal operations | General flow of the application |
| **Warning** | Abnormal situations | Non-critical issues |
| **Error** | Failures | Exceptions and errors |
| **Fatal** | Critical failures | Application crashes |

### **Change Log Level** (for debugging)

Edit `appsettings.Development.json`:
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug",  // Changed from Information to Debug
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.AspNetCore": "Warning",
        "Microsoft.EntityFrameworkCore": "Information"  // Show SQL queries
      }
    }
  }
}
```

---

## ?? Quick Reference

### **View Today's Logs**
```powershell
# Product Service
Get-Content src/Services/ProductService/logs/product-service-*.txt

# Order Service
Get-Content src/Services/OrderService/logs/order-service-*.txt

# Payment Service
Get-Content src/Services/PaymentService/logs/payment-service-*.txt

# User Service
Get-Content src/Services/UserService/logs/user-service-*.txt

# API Gateway
Get-Content src/ApiGateway/logs/api-gateway-*.txt
```

### **Watch Logs Live**
```powershell
Get-Content src/Services/OrderService/logs/order-service-*.txt -Wait -Tail 20
```

### **Find Errors**
```powershell
Select-String -Path "src/Services/*/logs/*.txt" -Pattern "Error|Exception"
```

### **Search Specific Error**
```powershell
Select-String -Path "src/Services/*/logs/*.txt" -Pattern "saga failed"
```

---

## ?? Notes

1. **Daily Rolling**: Log files are created daily with format: `service-name-YYYYMMDD.txt`
2. **Console Output**: Logs also appear in the console when running services
3. **No Rotation Limit**: By default, old logs are kept indefinitely (clean manually)
4. **Thread-Safe**: Serilog ensures logs from concurrent requests don't overlap
5. **UTF-8 Encoding**: Log files use UTF-8 encoding

---

## ?? Related Documentation

- [Circuit Breaker Implementation](CIRCUIT_BREAKER_IMPLEMENTATION.md)
- [CRUD Operations Flow](CRUD_OPERATIONS_FLOW.md)
- [API Documentation](API_DOCUMENTATION.md)

---

**Last Updated:** 2024-02-15  
**Version:** 1.0
