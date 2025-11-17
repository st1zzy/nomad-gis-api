using System.ComponentModel.DataAnnotations;

namespace nomad_gis_V2.Models;

public class Achievement
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, StringLength(100)]
    public string Code { get; set; } = string.Empty; // Например "OPEN_10_POINTS"

    [Required, StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public int GoalValue { get; set; } = 0;

    public int RewardPoints { get; set; } = 0;

    [StringLength(200)]
    public string? BadgeImageUrl { get; set; }

    public virtual ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();
}