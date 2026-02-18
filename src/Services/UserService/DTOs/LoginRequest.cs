using System.ComponentModel.DataAnnotations;

namespace UserService.DTOs;

/// <summary>
/// Request model for user login
/// </summary>
public class LoginRequest
{
    [Required]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    public string Password { get; set; } = string.Empty;
}
