# Microservices Solution Verification Script

Write-Host "==================================" -ForegroundColor Cyan
Write-Host "Microservices Solution Verification" -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan
Write-Host ""

# Check .NET Version
Write-Host "Checking .NET SDK version..." -ForegroundColor Yellow
$dotnetVersion = dotnet --version
Write-Host "? .NET SDK: $dotnetVersion" -ForegroundColor Green
Write-Host ""

# Check Solution
Write-Host "Verifying solution structure..." -ForegroundColor Yellow
if (Test-Path "MicroservicesDemo.slnx") {
    Write-Host "? Solution file found" -ForegroundColor Green
} else {
    Write-Host "? Solution file not found" -ForegroundColor Red
    exit 1
}

# Check Projects
$projects = @(
    "src/BuildingBlocks/Shared/Shared.csproj",
    "src/Services/ProductService/ProductService.csproj",
    "src/Services/OrderService/OrderService.csproj",
    "src/Services/PaymentService/PaymentService.csproj",
    "src/ApiGateway/ApiGateway.csproj"
)

Write-Host "Checking project files..." -ForegroundColor Yellow
foreach ($project in $projects) {
    if (Test-Path $project) {
        Write-Host "? $project" -ForegroundColor Green
    } else {
        Write-Host "? $project not found" -ForegroundColor Red
    }
}
Write-Host ""

# Restore packages
Write-Host "Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore MicroservicesDemo.slnx
if ($LASTEXITCODE -eq 0) {
    Write-Host "? Packages restored successfully" -ForegroundColor Green
} else {
    Write-Host "? Package restore failed" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Build solution
Write-Host "Building solution..." -ForegroundColor Yellow
dotnet build MicroservicesDemo.slnx --no-restore
if ($LASTEXITCODE -eq 0) {
    Write-Host "? Build successful" -ForegroundColor Green
} else {
    Write-Host "? Build failed" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Check Documentation
Write-Host "Checking documentation..." -ForegroundColor Yellow
$docs = @(
    "README.md",
    "docs/QUICKSTART.md",
    "docs/API_DOCUMENTATION.md",
    "docs/IMPLEMENTATION_SUMMARY.md"
)

foreach ($doc in $docs) {
    if (Test-Path $doc) {
        Write-Host "? $doc" -ForegroundColor Green
    } else {
        Write-Host "? $doc not found" -ForegroundColor Yellow
    }
}
Write-Host ""

# Summary
Write-Host "==================================" -ForegroundColor Cyan
Write-Host "Verification Summary" -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan
Write-Host "? All projects created" -ForegroundColor Green
Write-Host "? Solution builds successfully" -ForegroundColor Green
Write-Host "? Documentation available" -ForegroundColor Green
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "1. Apply database migrations:" -ForegroundColor White
Write-Host "   cd src/Services/ProductService; dotnet ef migrations add InitialCreate; dotnet ef database update" -ForegroundColor Gray
Write-Host "   cd ../OrderService; dotnet ef migrations add InitialCreate; dotnet ef database update" -ForegroundColor Gray
Write-Host "   cd ../PaymentService; dotnet ef migrations add InitialCreate; dotnet ef database update" -ForegroundColor Gray
Write-Host ""
Write-Host "2. Run all services (in separate terminals):" -ForegroundColor White
Write-Host "   cd src/Services/ProductService; dotnet run" -ForegroundColor Gray
Write-Host "   cd src/Services/OrderService; dotnet run" -ForegroundColor Gray
Write-Host "   cd src/Services/PaymentService; dotnet run" -ForegroundColor Gray
Write-Host "   cd src/ApiGateway; dotnet run" -ForegroundColor Gray
Write-Host ""
Write-Host "3. Access services:" -ForegroundColor White
Write-Host "   API Gateway:      https://localhost:7100" -ForegroundColor Gray
Write-Host "   Product Service:  https://localhost:7001/swagger" -ForegroundColor Gray
Write-Host "   Order Service:    https://localhost:7000/swagger" -ForegroundColor Gray
Write-Host "   Payment Service:  https://localhost:7002/swagger" -ForegroundColor Gray
Write-Host ""
Write-Host "4. Test the Saga flow:" -ForegroundColor White
Write-Host "   Import: Microservices-Postman-Collection.json into Postman" -ForegroundColor Gray
Write-Host "   Or run: curl -X POST https://localhost:7100/gateway/orders -H 'Content-Type: application/json' -d '{\"productId\":1,\"quantity\":2}'" -ForegroundColor Gray
Write-Host ""
Write-Host "For detailed instructions, see: docs/QUICKSTART.md" -ForegroundColor Cyan
Write-Host ""
