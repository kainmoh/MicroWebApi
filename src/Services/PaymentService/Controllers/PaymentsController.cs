using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaymentService.Data;
using PaymentService.Models;
using Shared.Constants;
using Shared.DTOs;
using Shared.Exceptions;
using Shared.Models;

namespace PaymentService.Controllers;

/// <summary>
/// Controller for managing payments
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PaymentsController : ControllerBase
{
    private readonly PaymentDbContext _context;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(PaymentDbContext context, ILogger<PaymentsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all payments
    /// </summary>
    /// <returns>List of payments</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<PaymentDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<PaymentDto>>>> GetAll()
    {
        _logger.LogInformation("Fetching all payments");
        var payments = await _context.Payments
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
        
        var paymentDtos = payments.Select(MapToDto).ToList();
        return Ok(ApiResponse<List<PaymentDto>>.SuccessResponse(paymentDtos));
    }

    /// <summary>
    /// Get payment by ID
    /// </summary>
    /// <param name="id">Payment ID</param>
    /// <returns>Payment details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<PaymentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PaymentDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> GetById(int id)
    {
        _logger.LogInformation("Fetching payment with ID: {PaymentId}", id);
        var payment = await _context.Payments.FindAsync(id);

        if (payment == null)
        {
            _logger.LogWarning("Payment not found: {PaymentId}", id);
            throw new NotFoundException($"Payment with ID {id} not found");
        }

        return Ok(ApiResponse<PaymentDto>.SuccessResponse(MapToDto(payment)));
    }

    /// <summary>
    /// Get payments by Order ID
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <returns>List of payments for the order</returns>
    [HttpGet("order/{orderId}")]
    [ProducesResponseType(typeof(ApiResponse<List<PaymentDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<PaymentDto>>>> GetByOrderId(int orderId)
    {
        _logger.LogInformation("Fetching payments for OrderId: {OrderId}", orderId);
        var payments = await _context.Payments
            .Where(p => p.OrderId == orderId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
        
        var paymentDtos = payments.Select(MapToDto).ToList();
        return Ok(ApiResponse<List<PaymentDto>>.SuccessResponse(paymentDtos));
    }

    /// <summary>
    /// Create a new payment (without processing)
    /// </summary>
    /// <param name="dto">Payment creation details</param>
    /// <returns>Created payment</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PaymentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<PaymentDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> Create([FromBody] CreatePaymentDto dto)
    {
        _logger.LogInformation("Creating payment for OrderId: {OrderId}, Amount: {Amount}", 
            dto.OrderId, dto.Amount);

        var payment = new Payment
        {
            OrderId = dto.OrderId,
            Amount = dto.Amount,
            PaymentMethod = dto.PaymentMethod,
            Status = PaymentStatus.Pending,
            TransactionId = GenerateTransactionId(),
            CreatedAt = DateTime.UtcNow
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Payment created: {PaymentId}", payment.Id);
        return CreatedAtAction(
            nameof(GetById),
            new { id = payment.Id },
            ApiResponse<PaymentDto>.SuccessResponse(MapToDto(payment), "Payment created successfully", 201)
        );
    }

    /// <summary>
    /// Process payment (called by Order Service during Saga)
    /// </summary>
    /// <param name="dto">Payment processing details</param>
    /// <returns>Processed payment</returns>
    [HttpPost("process")]
    [ProducesResponseType(typeof(ApiResponse<PaymentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PaymentDto>), StatusCodes.Status402PaymentRequired)]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> ProcessPayment([FromBody] ProcessPaymentDto dto)
    {
        _logger.LogInformation("Processing payment for OrderId: {OrderId}, Amount: {Amount}", 
            dto.OrderId, dto.Amount);

        // Simulate payment gateway processing
        var (success, failureReason) = await SimulatePaymentGateway(dto);

        var payment = new Payment
        {
            OrderId = dto.OrderId,
            Amount = dto.Amount,
            PaymentMethod = dto.PaymentMethod,
            Status = success ? PaymentStatus.PaymentDone : PaymentStatus.Failed,
            TransactionId = GenerateTransactionId(),
            CreatedAt = DateTime.UtcNow,
            ProcessedAt = DateTime.UtcNow,
            FailureReason = failureReason
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        if (!success)
        {
            _logger.LogWarning("Payment failed for OrderId: {OrderId}. Reason: {Reason}", 
                dto.OrderId, failureReason);
            throw new PaymentFailedException($"Payment processing failed: {failureReason}");
        }

        _logger.LogInformation("Payment processed successfully: {PaymentId} for OrderId: {OrderId}", 
            payment.Id, payment.OrderId);
        
        return Ok(ApiResponse<PaymentDto>.SuccessResponse(
            MapToDto(payment), 
            "Payment processed successfully"
        ));
    }

    /// <summary>
    /// Delete a payment
    /// </summary>
    /// <param name="id">Payment ID</param>
    /// <returns>Deletion confirmation</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        _logger.LogInformation("Deleting payment: {PaymentId}", id);
        
        var payment = await _context.Payments.FindAsync(id);
        if (payment == null)
        {
            _logger.LogWarning("Payment not found for deletion: {PaymentId}", id);
            throw new NotFoundException($"Payment with ID {id} not found");
        }

        _context.Payments.Remove(payment);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Payment deleted successfully: {PaymentId}", id);
        return Ok(ApiResponse<bool>.SuccessResponse(true, "Payment deleted successfully"));
    }

    /// <summary>
    /// Simulate payment gateway processing
    /// In production, this would integrate with actual payment gateways (Stripe, PayPal, etc.)
    /// </summary>
    private async Task<(bool success, string? failureReason)> SimulatePaymentGateway(ProcessPaymentDto dto)
    {
        // Simulate network delay
        await Task.Delay(Random.Shared.Next(100, 500));

        // Simulate 95% success rate
        var random = Random.Shared.Next(100);
        
        if (random < 95)
        {
            _logger.LogInformation("Payment gateway simulation: SUCCESS for OrderId {OrderId}", dto.OrderId);
            return (true, null);
        }
        else
        {
            var failureReasons = new[]
            {
                "Insufficient funds",
                "Card declined",
                "Invalid card number",
                "Card expired",
                "Gateway timeout"
            };
            
            var reason = failureReasons[Random.Shared.Next(failureReasons.Length)];
            _logger.LogWarning("Payment gateway simulation: FAILED for OrderId {OrderId}. Reason: {Reason}", 
                dto.OrderId, reason);
            
            return (false, reason);
        }
    }

    /// <summary>
    /// Generate a unique transaction ID
    /// </summary>
    private static string GenerateTransactionId()
    {
        return $"TXN-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
    }

    private static PaymentDto MapToDto(Payment payment) => new()
    {
        Id = payment.Id,
        OrderId = payment.OrderId,
        Amount = payment.Amount,
        PaymentMethod = payment.PaymentMethod,
        Status = payment.Status,
        TransactionId = payment.TransactionId,
        CreatedAt = payment.CreatedAt,
        ProcessedAt = payment.ProcessedAt
    };
}
