using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UserService.Data;
using UserService.DTOs;
using UserService.Models;

namespace UserService.Services;

/// <summary>
/// Service for authentication and user management
/// </summary>
public class AuthService : IAuthService
{
    private readonly UserDbContext _context;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserDbContext context,
        IOptions<JwtSettings> jwtSettings,
        ILogger<AuthService> logger)
    {
        _context = context;
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive);

            if (user == null)
            {
                _logger.LogWarning("Login failed: User {Username} not found", request.Username);
                return null;
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed: Invalid password for user {Username}", request.Username);
                return null;
            }

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user);
            var expiresAt = DateTime.UtcNow.AddHours(_jwtSettings.ExpirationHours);

            _logger.LogInformation("User {Username} logged in successfully", user.Username);

            return new AuthResponse
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                Token = token,
                ExpiresAt = expiresAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user {Username}", request.Username);
            throw;
        }
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        try
        {
            // Check if username exists
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                _logger.LogWarning("Registration failed: Username {Username} already exists", request.Username);
                return null;
            }

            // Check if email exists
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                _logger.LogWarning("Registration failed: Email {Email} already exists", request.Email);
                return null;
            }

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Role = "User", // Default role
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user);
            var expiresAt = DateTime.UtcNow.AddHours(_jwtSettings.ExpirationHours);

            _logger.LogInformation("User {Username} registered successfully", user.Username);

            return new AuthResponse
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                Token = token,
                ExpiresAt = expiresAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for user {Username}", request.Username);
            throw;
        }
    }

    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        return user == null ? null : MapToDto(user);
    }

    public async Task<UserDto?> GetUserByUsernameAsync(string username)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username);
        return user == null ? null : MapToDto(user);
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            tokenHandler.ValidateToken(token, validationParameters, out _);
            return await Task.FromResult(true);
        }
        catch
        {
            return false;
        }
    }

    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.GivenName, user.FirstName),
            new Claim(ClaimTypes.Surname, user.LastName),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("userId", user.Id.ToString()),
            new Claim("username", user.Username)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(_jwtSettings.ExpirationHours),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private static UserDto MapToDto(User user)
    {
        return new UserDto
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
    }
}
