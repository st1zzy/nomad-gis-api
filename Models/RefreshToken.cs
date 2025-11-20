using System.ComponentModel.DataAnnotations;

namespace nomad_gis_V2.Models;

public class RefreshToken
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(512)]
    public string Token { get; set; } = string.Empty;

    [Required]
    public DateTime Expires { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RevorkedAt { get; set; }

    [Required, MaxLength(200)]
    public string DeviceId { get; set; } = string.Empty;

    [Required]
    public Guid UserId { get; set; }

    public User User { get; set; } = null!;
}
