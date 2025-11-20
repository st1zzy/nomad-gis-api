namespace nomad_gis_V2.DTOs.Achievements;

public class AchievementUpdateRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int? RewardPoints { get; set; }
    public IFormFile? BadgeFile { get; set; }
}
