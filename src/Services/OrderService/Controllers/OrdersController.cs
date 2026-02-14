using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Models;
using OrderService.Services;
using Shared.DTOs;
using Shared.Exceptions;
using Shared.Models;

namespace OrderService.Controllers;

/// <summary>
/// Controller for managing orders and orchestrating the order saga
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class OrdersController : ControllerBase
{
    private readonly OrderDbContext _context;
    private readonly IOrderSagaOrchestrator _sagaOrchestrator;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        OrderDbContext context,
        IOrderSagaOrchestrator sagaOrchestrator,
        ILogger<OrdersController> logger)
    {
        _context = context;
        _sagaOrchestrator = sagaOrchestrator;
        _logger = logger;
    }

    /// <summary>
    /// Get all orders
    /// </summary>
    /// <returns>List of orders</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<OrderDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<OrderDto>>>> GetAll()
    {
        _logger.LogInformation("Fetching all orders");
        var orders = await _context.Orders
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
        
        var orderDtos = orders.Select(MapToDto).ToList();
        return Ok(ApiResponse<List<OrderDto>>.SuccessResponse(orderDtos));
    }

    /// <summary>
    /// Get order by ID
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>Order details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<OrderDto>>> GetById(int id)
    {
        _logger.LogInformation("Fetching order with ID: {OrderId}", id);
        var order = await _context.Orders.FindAsync(id);

        if (order == null)
        {
            _logger.LogWarning("Order not found: {OrderId}", id);
            throw new NotFoundException($"Order with ID {id} not found");
        }

        return Ok(ApiResponse<OrderDto>.SuccessResponse(MapToDto(order)));
    }

    /// <summary>
    /// Create a new order (triggers Saga orchestration)
    /// </summary>
    /// <param name="dto">Order creation details</param>
    /// <returns>Created order with final status</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<ApiResponse<OrderDto>>> CreateOrder([FromBody] CreateOrderDto dto)
    {
        _logger.LogInformation("Creating order for ProductId: {ProductId}, Quantity: {Quantity}", 
            dto.ProductId, dto.Quantity);

        // Execute saga orchestration
        var order = await _sagaOrchestrator.ExecuteOrderSagaAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = order.Id },
            ApiResponse<OrderDto>.SuccessResponse(order, "Order created and processed successfully", 201)
        );
    }

    /// <summary>
    /// Update order status manually
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <param name="dto">Status update details</param>
    /// <returns>Updated order</returns>
    [HttpPut("{id}/status")]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<OrderDto>>> UpdateStatus(int id, [FromBody] UpdateOrderStatusDto dto)
    {
        _logger.LogInformation("Updating status for order: {OrderId} to {Status}", id, dto.Status);
        
        var order = await _context.Orders.FindAsync(id);
        if (order == null)
        {
            _logger.LogWarning("Order not found for status update: {OrderId}", id);
            throw new NotFoundException($"Order with ID {id} not found");
        }

        order.Status = dto.Status;
        if (dto.Status == "Completed" && order.CompletedAt == null)
        {
            order.CompletedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Order status updated: {OrderId} -> {Status}", id, dto.Status);

        return Ok(ApiResponse<OrderDto>.SuccessResponse(MapToDto(order), "Order status updated successfully"));
    }

    /// <summary>
    /// Delete an order
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>Deletion confirmation</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        _logger.LogInformation("Deleting order: {OrderId}", id);
        
        var order = await _context.Orders.FindAsync(id);
        if (order == null)
        {
            _logger.LogWarning("Order not found for deletion: {OrderId}", id);
            throw new NotFoundException($"Order with ID {id} not found");
        }

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Order deleted successfully: {OrderId}", id);
        return Ok(ApiResponse<bool>.SuccessResponse(true, "Order deleted successfully"));
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
