#!/usr/bin/env pwsh
# Start all microservices in separate PowerShell windows

Write-Host "?? Starting All Microservices..." -ForegroundColor Green
Write-Host ""

# Define services with their paths and ports
$services = @(
    @{
        Name = "ProductService"
        Path = "src\Services\ProductService"
        Port = "7001"
        Color = "Cyan"
    },
    @{
        Name = "PaymentService"
        Path = "src\Services\PaymentService"
        Port = "7002"
        Color = "Yellow"
    },
    @{
        Name = "OrderService"
        Path = "src\Services\OrderService"
        Port = "7000"
        Color = "Magenta"
    },
    @{
        Name = "ApiGateway"
        Path = "src\ApiGateway"
        Port = "7100"
        Color = "Green"
    }
)

Write-Host "?? Services Configuration:" -ForegroundColor White
Write-Host "???????????????????????????????????????????????????????????" -ForegroundColor Gray
foreach ($service in $services) {
    Write-Host "  ? $($service.Name.PadRight(20)) https://localhost:$($service.Port)" -ForegroundColor $service.Color
}
Write-Host "???????????????????????????????????????????????????????????" -ForegroundColor Gray
Write-Host ""

# Start each service in a new PowerShell window
foreach ($service in $services) {
    Write-Host "?? Starting $($service.Name)..." -ForegroundColor $service.Color
    
    $command = "cd '$($service.Path)'; " +
               "Write-Host '?? $($service.Name) Starting...' -ForegroundColor $($service.Color); " +
               "dotnet run; " +
               "Write-Host 'Press any key to close...' -ForegroundColor Red; " +
               "`$null = `$Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown')"
    
    Start-Process powershell -ArgumentList "-NoExit", "-Command", $command
    
    # Small delay to avoid startup conflicts
    Start-Sleep -Milliseconds 500
}

Write-Host ""
Write-Host "? All services are starting!" -ForegroundColor Green
Write-Host ""
Write-Host "?? Swagger Endpoints:" -ForegroundColor White
Write-Host "???????????????????????????????????????????????????????????" -ForegroundColor Gray
Write-Host "  ?? Product Service:  " -NoNewline -ForegroundColor Cyan
Write-Host "https://localhost:7001/swagger"
Write-Host "  ?? Payment Service:  " -NoNewline -ForegroundColor Yellow
Write-Host "https://localhost:7002/swagger"
Write-Host "  ?? Order Service:    " -NoNewline -ForegroundColor Magenta
Write-Host "https://localhost:7000/swagger"
Write-Host "  ?? API Gateway:      " -NoNewline -ForegroundColor Green
Write-Host "https://localhost:7100"
Write-Host "???????????????????????????????????????????????????????????" -ForegroundColor Gray
Write-Host ""
Write-Host "?? Tips:" -ForegroundColor Yellow
Write-Host "  • Wait 10-15 seconds for all services to start"
Write-Host "  • Check each PowerShell window for startup logs"
Write-Host "  • Access Swagger UI through the URLs above"
Write-Host "  • Use API Gateway (7100) to route requests to services"
Write-Host ""
Write-Host "?? Quick Test:" -ForegroundColor Cyan
Write-Host "  curl https://localhost:7001/api/products"
Write-Host ""
Write-Host "Press Ctrl+C to exit this script..." -ForegroundColor Gray
Write-Host ""

# Keep script running
try {
    while ($true) {
        Start-Sleep -Seconds 1
    }
}
finally {
    Write-Host ""
    Write-Host "??  Note: Service windows will remain open. Close them manually." -ForegroundColor Yellow
}
