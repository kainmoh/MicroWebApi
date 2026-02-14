namespace Shared.DTOs;

/// <summary>
/// Order Data Transfer Object
/// </summary>
public class OrderDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

/// <summary>
/// DTO for creating a new order
/// </summary>
public class CreateOrderDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

/// <summary>
/// DTO for updating order status
/// </summary>
public class UpdateOrderStatusDto
{
    public int OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
}
