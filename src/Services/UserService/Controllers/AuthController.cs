using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;
using UserService.DTOs;
using UserService.Services;

namespace UserService.Controllers;

/// <summary>
/// Controller for authentication operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Login with username and password
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request)
    {
        _logger.LogInformation("Login attempt for user: {Username}", request.Username);

        var result = await _authService.LoginAsync(request);

        if (result == null)
        {
            _logger.LogWarning("Login failed for user: {Username}", request.Username);
            return Unauthorized(ApiResponse<object>.FailureResponse("Invalid username or password", 401));
        }

        return Ok(ApiResponse<AuthResponse>.SuccessResponse(result, "Login successful"));
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register([FromBody] RegisterRequest request)
    {
        _logger.LogInformation("Registration attempt for user: {Username}", request.Username);

        var result = await _authService.RegisterAsync(request);

        if (result == null)
        {
            _logger.LogWarning("Registration failed for user: {Username}", request.Username);
            return BadRequest(ApiResponse<object>.FailureResponse("Username or email already exists", 400));
        }

        return CreatedAtAction(
            nameof(GetProfile),
            new { id = result.UserId },
            ApiResponse<AuthResponse>.SuccessResponse(result, "Registration successful", 201));
    }

    /// <summary>
    /// Get current user profile
    /// </summary>
    [HttpGet("profile")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetProfile()
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
        {
            return Unauthorized(ApiResponse<object>.FailureResponse("User not authenticated", 401));
        }

        var user = await _authService.GetUserByUsernameAsync(username);

        if (user == null)
        {
            return NotFound(ApiResponse<object>.FailureResponse("User not found", 404));
        }

        return Ok(ApiResponse<UserDto>.SuccessResponse(user, "Profile retrieved successfully"));
    }

    /// <summary>
    /// Validate JWT token
    /// </summary>
    [HttpPost("validate-token")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    public async Task<ActionResult<ApiResponse<bool>>> ValidateToken([FromBody] string token)
    {
        var isValid = await _authService.ValidateTokenAsync(token);
        return Ok(ApiResponse<bool>.SuccessResponse(isValid, isValid ? "Token is valid" : "Token is invalid"));
    }
}
