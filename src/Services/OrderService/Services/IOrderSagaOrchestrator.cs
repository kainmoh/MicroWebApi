using Shared.DTOs;

namespace OrderService.Services;

/// <summary>
/// Interface for Saga orchestrator that manages distributed transactions
/// </summary>
public interface IOrderSagaOrchestrator
{
    /// <summary>
    /// Executes the order saga workflow
    /// </summary>
    /// <param name="orderDto">Order creation details</param>
    /// <returns>Created order with final status</returns>
    Task<OrderDto> ExecuteOrderSagaAsync(CreateOrderDto orderDto);
}
