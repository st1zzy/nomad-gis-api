namespace nomad_gis_V2.DTOs.Achievements;

public class AchievementResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int RewardPoints { get; set; }
    public string? BadgeImageUrl { get; set; }
}
