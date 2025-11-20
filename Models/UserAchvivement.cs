namespace nomad_gis_V2.Models;

public class UserAchievement
{
    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;

    public Guid AchievementId { get; set; } = Guid.Empty;
    public virtual Achievement Achievement { get; set; } = null!;

    public int ProgressValue { get; set; } = 0;
    public bool IsCompleted { get; set; } = false;
    public DateTime? CompletedAt { get; set; }
}
