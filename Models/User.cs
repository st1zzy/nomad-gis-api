using System.ComponentModel.DataAnnotations;

namespace nomad_gis_V2.Models;

public class User
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Range(-90.0, 90.0)]
    public double? Latitude { get; set; }

    [Range(-180.0, 180.0)]
    public double? Longitude { get; set; }

    public int Experience { get; set; } = 0;
    public int Level { get; set; } = 1;

    [Required, MaxLength(50)]
    public string Role { get; set; } = "User"; // По умолчанию "User"
    public string? AvatarUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    // Навигационные свойства
    public virtual ICollection<UserMapProgress> MapProgress { get; set; } = new List<UserMapProgress>();
    public virtual ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
    public List<RefreshToken> RefreshTokens { get; set; } = new(5);
}
