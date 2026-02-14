using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Models;
using Shared.DTOs;
using Shared.Exceptions;
using Shared.Models;

namespace ProductService.Controllers;

/// <summary>
/// Controller for managing products
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly ProductDbContext _context;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(ProductDbContext context, ILogger<ProductsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all products
    /// </summary>
    /// <returns>List of products</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<ProductDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<ProductDto>>>> GetAll()
    {
        _logger.LogInformation("Fetching all products");
        var products = await _context.Products.ToListAsync();
        var productDtos = products.Select(MapToDto).ToList();
        
        return Ok(ApiResponse<List<ProductDto>>.SuccessResponse(productDtos));
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Product details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetById(int id)
    {
        _logger.LogInformation("Fetching product with ID: {ProductId}", id);
        var product = await _context.Products.FindAsync(id);
        
        if (product == null)
        {
            _logger.LogWarning("Product not found: {ProductId}", id);
            throw new NotFoundException($"Product with ID {id} not found");
        }

        return Ok(ApiResponse<ProductDto>.SuccessResponse(MapToDto(product)));
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    /// <param name="dto">Product creation details</param>
    /// <returns>Created product</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Create([FromBody] CreateProductDto dto)
    {
        _logger.LogInformation("Creating new product: {ProductName}", dto.Name);
        
        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Quantity = dto.Quantity,
            Category = dto.Category,
            CreatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Product created successfully: {ProductId}", product.Id);
        return CreatedAtAction(
            nameof(GetById), 
            new { id = product.Id }, 
            ApiResponse<ProductDto>.SuccessResponse(MapToDto(product), "Product created successfully", 201)
        );
    }

    /// <summary>
    /// Update an existing product
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="dto">Product update details</param>
    /// <returns>Updated product</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Update(int id, [FromBody] UpdateProductDto dto)
    {
        _logger.LogInformation("Updating product: {ProductId}", id);
        
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            _logger.LogWarning("Product not found for update: {ProductId}", id);
            throw new NotFoundException($"Product with ID {id} not found");
        }

        if (!string.IsNullOrEmpty(dto.Name)) product.Name = dto.Name;
        if (!string.IsNullOrEmpty(dto.Description)) product.Description = dto.Description;
        if (dto.Price.HasValue) product.Price = dto.Price.Value;
        if (dto.Quantity.HasValue) product.Quantity = dto.Quantity.Value;
        if (!string.IsNullOrEmpty(dto.Category)) product.Category = dto.Category;
        
        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Product updated successfully: {ProductId}", product.Id);

        return Ok(ApiResponse<ProductDto>.SuccessResponse(MapToDto(product), "Product updated successfully"));
    }

    /// <summary>
    /// Delete a product
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Deletion confirmation</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        _logger.LogInformation("Deleting product: {ProductId}", id);
        
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            _logger.LogWarning("Product not found for deletion: {ProductId}", id);
            throw new NotFoundException($"Product with ID {id} not found");
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Product deleted successfully: {ProductId}", id);
        return Ok(ApiResponse<bool>.SuccessResponse(true, "Product deleted successfully"));
    }

    /// <summary>
    /// Update product inventory (reduce quantity)
    /// </summary>
    /// <param name="dto">Inventory update details</param>
    /// <returns>Update confirmation</returns>
    [HttpPost("update-inventory")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateInventory([FromBody] UpdateInventoryDto dto)
    {
        _logger.LogInformation("Updating inventory for product: {ProductId}, Quantity: {Quantity}", dto.ProductId, dto.Quantity);
        
        var product = await _context.Products.FindAsync(dto.ProductId);
        if (product == null)
        {
            _logger.LogWarning("Product not found for inventory update: {ProductId}", dto.ProductId);
            throw new NotFoundException($"Product with ID {dto.ProductId} not found");
        }

        if (product.Quantity < dto.Quantity)
        {
            _logger.LogWarning("Insufficient inventory for product: {ProductId}. Available: {Available}, Required: {Required}", 
                dto.ProductId, product.Quantity, dto.Quantity);
            throw new InsufficientInventoryException(
                $"Insufficient inventory for product {product.Name}. Available: {product.Quantity}, Required: {dto.Quantity}"
            );
        }

        product.Quantity -= dto.Quantity;
        product.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Inventory updated successfully for product: {ProductId}. New quantity: {Quantity}", 
            dto.ProductId, product.Quantity);
        return Ok(ApiResponse<bool>.SuccessResponse(true, "Inventory updated successfully"));
    }

    /// <summary>
    /// Rollback inventory (restore quantity) - Compensation transaction
    /// </summary>
    /// <param name="dto">Inventory rollback details</param>
    /// <returns>Rollback confirmation</returns>
    [HttpPost("rollback-inventory")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> RollbackInventory([FromBody] UpdateInventoryDto dto)
    {
        _logger.LogInformation("Rolling back inventory for product: {ProductId}, Quantity: {Quantity}", dto.ProductId, dto.Quantity);
        
        var product = await _context.Products.FindAsync(dto.ProductId);
        if (product == null)
        {
            _logger.LogWarning("Product not found for inventory rollback: {ProductId}", dto.ProductId);
            throw new NotFoundException($"Product with ID {dto.ProductId} not found");
        }

        product.Quantity += dto.Quantity;
        product.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Inventory rollback completed for product: {ProductId}. New quantity: {Quantity}", 
            dto.ProductId, product.Quantity);
        return Ok(ApiResponse<bool>.SuccessResponse(true, "Inventory rollback successful"));
    }

    private static ProductDto MapToDto(Product product) => new()
    {
        Id = product.Id,
        Name = product.Name,
        Description = product.Description,
        Price = product.Price,
        Quantity = product.Quantity,
        Category = product.Category,
        CreatedAt = product.CreatedAt
    };
}
