using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Models;
using UserService.Data;
using UserService.DTOs;

namespace UserService.Controllers;

/// <summary>
/// Controller for user management (Admin only)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly UserDbContext _context;
    private readonly ILogger<UsersController> _logger;

    public UsersController(UserDbContext context, ILogger<UsersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all users (Admin only)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<UserDto>>), 200)]
    public async Task<ActionResult<ApiResponse<List<UserDto>>>> GetAllUsers()
    {
        _logger.LogInformation("Fetching all users");

        var users = await _context.Users
            .Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Role = u.Role,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                LastLoginAt = u.LastLoginAt
            })
            .ToListAsync();

        return Ok(ApiResponse<List<UserDto>>.SuccessResponse(users, $"Retrieved {users.Count} users"));
    }

    /// <summary>
    /// Get user by ID (Admin only)
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetUser(int id)
    {
        _logger.LogInformation("Fetching user with ID: {UserId}", id);

        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound(ApiResponse<object>.FailureResponse($"User with ID {id} not found", 404));
        }

        var userDto = new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };

        return Ok(ApiResponse<UserDto>.SuccessResponse(userDto, "User retrieved successfully"));
    }

    /// <summary>
    /// Deactivate user (Admin only)
    /// </summary>
    [HttpPut("{id}/deactivate")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<ActionResult<ApiResponse<object>>> DeactivateUser(int id)
    {
        _logger.LogInformation("Deactivating user with ID: {UserId}", id);

        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound(ApiResponse<object>.FailureResponse($"User with ID {id} not found", 404));
        }

        user.IsActive = false;
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<object>.SuccessResponse(null, "User deactivated successfully"));
    }

    /// <summary>
    /// Activate user (Admin only)
    /// </summary>
    [HttpPut("{id}/activate")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<ActionResult<ApiResponse<object>>> ActivateUser(int id)
    {
        _logger.LogInformation("Activating user with ID: {UserId}", id);

        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound(ApiResponse<object>.FailureResponse($"User with ID {id} not found", 404));
        }

        user.IsActive = true;
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<object>.SuccessResponse(null, "User activated successfully"));
    }
}
