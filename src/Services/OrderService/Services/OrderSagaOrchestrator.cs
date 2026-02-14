using OrderService.Data;
using OrderService.Models;
using Shared.Constants;
using Shared.DTOs;
using Shared.Exceptions;
using Shared.Models;
using System.Text;
using System.Text.Json;

namespace OrderService.Services;

/// <summary>
/// Saga Orchestrator implementing the orchestration-based saga pattern
/// Manages the distributed transaction flow for order processing
/// </summary>
public class OrderSagaOrchestrator : IOrderSagaOrchestrator
{
    private readonly OrderDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<OrderSagaOrchestrator> _logger;
    private readonly string _productServiceUrl;
    private readonly string _paymentServiceUrl;

    public OrderSagaOrchestrator(
        OrderDbContext context,
        IHttpClientFactory httpClientFactory,
        ILogger<OrderSagaOrchestrator> logger,
        IConfiguration configuration)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _productServiceUrl = configuration["Services:ProductService"] ?? "https://localhost:7001";
        _paymentServiceUrl = configuration["Services:PaymentService"] ?? "https://localhost:7002";
    }

    /// <summary>
    /// Executes the order saga with compensation logic
    /// Flow: Create Order ? Update Inventory ? Process Payment ? Confirm Order
    /// On failure: Rollback inventory and mark order as failed
    /// </summary>
    public async Task<OrderDto> ExecuteOrderSagaAsync(CreateOrderDto orderDto)
    {
        Order? order = null;
        var inventoryUpdated = false;
        var sagaId = Guid.NewGuid();

        try
        {
            _logger.LogInformation("=== Starting Saga {SagaId} for ProductId: {ProductId}, Quantity: {Quantity} ===", 
                sagaId, orderDto.ProductId, orderDto.Quantity);

            // ====================================================================
            // STEP 1: Get Product Details and Calculate Total
            // ====================================================================
            _logger.LogInformation("[Saga {SagaId}] Step 1: Fetching product details", sagaId);
            var product = await GetProductAsync(orderDto.ProductId);
            var totalAmount = product.Price * orderDto.Quantity;
            _logger.LogInformation("[Saga {SagaId}] Product found: {ProductName}, Price: {Price}, Total: {Total}", 
                sagaId, product.Name, product.Price, totalAmount);

            // ====================================================================
            // STEP 2: Create Order (Pending State)
            // ====================================================================
            _logger.LogInformation("[Saga {SagaId}] Step 2: Creating order in Pending state", sagaId);
            order = new Order
            {
                ProductId = orderDto.ProductId,
                Quantity = orderDto.Quantity,
                TotalAmount = totalAmount,
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            _logger.LogInformation("[Saga {SagaId}] Order created with ID: {OrderId}", sagaId, order.Id);

            // ====================================================================
            // STEP 3: Update Inventory (Deduct Stock)
            // ====================================================================
            _logger.LogInformation("[Saga {SagaId}] Step 3: Updating inventory for ProductId: {ProductId}", 
                sagaId, orderDto.ProductId);
            await UpdateInventoryAsync(orderDto.ProductId, orderDto.Quantity);
            inventoryUpdated = true;
            
            order.Status = OrderStatus.Ordered;
            await _context.SaveChangesAsync();
            _logger.LogInformation("[Saga {SagaId}] Inventory updated. Order status: {Status}", 
                sagaId, order.Status);

            // ====================================================================
            // STEP 4: Process Payment
            // ====================================================================
            _logger.LogInformation("[Saga {SagaId}] Step 4: Processing payment for OrderId: {OrderId}, Amount: {Amount}", 
                sagaId, order.Id, totalAmount);
            await ProcessPaymentAsync(order.Id, totalAmount);
            
            order.Status = OrderStatus.PaymentProcessed;
            await _context.SaveChangesAsync();
            _logger.LogInformation("[Saga {SagaId}] Payment processed. Order status: {Status}", 
                sagaId, order.Status);

            // ====================================================================
            // STEP 5: Confirm Order (Final State)
            // ====================================================================
            _logger.LogInformation("[Saga {SagaId}] Step 5: Confirming order", sagaId);
            order.Status = OrderStatus.Completed;
            order.CompletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("=== Saga {SagaId} COMPLETED Successfully. OrderId: {OrderId}, Status: {Status} ===", 
                sagaId, order.Id, order.Status);

            return MapToDto(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "=== Saga {SagaId} FAILED: {ErrorMessage} ===", sagaId, ex.Message);

            // ====================================================================
            // COMPENSATION LOGIC: Rollback Transactions
            // ====================================================================
            if (order != null)
            {
                try
                {
                    _logger.LogWarning("[Saga {SagaId}] Starting compensation transactions", sagaId);

                    // Rollback inventory if it was updated
                    if (inventoryUpdated)
                    {
                        _logger.LogInformation("[Saga {SagaId}] Compensation: Rolling back inventory for ProductId: {ProductId}", 
                            sagaId, orderDto.ProductId);
                        await RollbackInventoryAsync(orderDto.ProductId, orderDto.Quantity);
                        _logger.LogInformation("[Saga {SagaId}] Compensation: Inventory rollback completed", sagaId);
                    }

                    // Mark order as failed
                    order.Status = OrderStatus.Failed;
                    order.FailureReason = ex.Message;
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("=== Saga {SagaId} marked as FAILED. OrderId: {OrderId} ===", 
                        sagaId, order.Id);
                }
                catch (Exception rollbackEx)
                {
                    _logger.LogError(rollbackEx, 
                        "[Saga {SagaId}] CRITICAL: Compensation transaction failed for OrderId: {OrderId}", 
                        sagaId, order.Id);
                }
            }

            throw new ServiceCommunicationException($"Order saga failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Fetch product details from Product Service
    /// </summary>
    private async Task<ProductDto> GetProductAsync(int productId)
    {
        var client = _httpClientFactory.CreateClient("ProductService");
        var response = await client.GetAsync($"{_productServiceUrl}/api/products/{productId}");

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to fetch product {ProductId}: {Error}", productId, errorContent);
            throw new NotFoundException($"Product with ID {productId} not found");
        }

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<ProductDto>>(content, 
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return apiResponse?.Data ?? throw new NotFoundException($"Product with ID {productId} not found");
    }

    /// <summary>
    /// Update inventory in Product Service
    /// </summary>
    private async Task UpdateInventoryAsync(int productId, int quantity)
    {
        var client = _httpClientFactory.CreateClient("ProductService");
        var dto = new UpdateInventoryDto { ProductId = productId, Quantity = quantity };
        var jsonContent = JsonSerializer.Serialize(dto);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        var response = await client.PostAsync($"{_productServiceUrl}/api/products/update-inventory", content);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to update inventory for Product {ProductId}: {Error}", productId, errorContent);
            throw new InsufficientInventoryException($"Failed to update inventory: {errorContent}");
        }
    }

    /// <summary>
    /// Process payment via Payment Service
    /// </summary>
    private async Task ProcessPaymentAsync(int orderId, decimal amount)
    {
        var client = _httpClientFactory.CreateClient("PaymentService");
        var dto = new ProcessPaymentDto 
        { 
            OrderId = orderId, 
            Amount = amount, 
            PaymentMethod = PaymentMethod.CreditCard 
        };
        var jsonContent = JsonSerializer.Serialize(dto);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        var response = await client.PostAsync($"{_paymentServiceUrl}/api/payments/process", content);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Payment processing failed for Order {OrderId}: {Error}", orderId, errorContent);
            throw new PaymentFailedException($"Payment processing failed: {errorContent}");
        }
    }

    /// <summary>
    /// Compensation: Rollback inventory in Product Service
    /// </summary>
    private async Task RollbackInventoryAsync(int productId, int quantity)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ProductService");
            var dto = new UpdateInventoryDto { ProductId = productId, Quantity = quantity };
            var jsonContent = JsonSerializer.Serialize(dto);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{_productServiceUrl}/api/products/rollback-inventory", content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to rollback inventory for Product {ProductId}: {Error}", 
                    productId, errorContent);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during inventory rollback for Product {ProductId}", productId);
        }
    }

    private static OrderDto MapToDto(Order order) => new()
    {
        Id = order.Id,
        ProductId = order.ProductId,
        Quantity = order.Quantity,
        TotalAmount = order.TotalAmount,
        Status = order.Status,
        CreatedAt = order.CreatedAt,
        CompletedAt = order.CompletedAt
    };
}
