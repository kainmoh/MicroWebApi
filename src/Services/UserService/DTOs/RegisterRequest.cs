using System.ComponentModel.DataAnnotations;

namespace UserService.DTOs;

/// <summary>
/// Request model for user registration
/// </summary>
public class RegisterRequest
{
    [Required]
    [MinLength(3)]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [MinLength(6)]
    [MaxLength(100)]
    public string Password { get; set; } = string.Empty;
    
    [Required]
    [Compare("Password", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;
}
