using System.ComponentModel.DataAnnotations;

namespace nomad_gis_V2.DTOs.Auth;

public class RegisterRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;
    
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string DeviceId { get; set; } = string.Empty;
}