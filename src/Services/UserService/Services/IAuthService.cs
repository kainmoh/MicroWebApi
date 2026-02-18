using UserService.DTOs;

namespace UserService.Services;

/// <summary>
/// Interface for authentication service    
/// </summary>
public interface IAuthService
{
    Task<AuthResponse?> LoginAsync(LoginRequest request);
    Task<AuthResponse?> RegisterAsync(RegisterRequest request);
    Task<UserDto?> GetUserByIdAsync(int id);
    Task<UserDto?> GetUserByUsernameAsync(string username);
    Task<bool> ValidateTokenAsync(string token);
}
