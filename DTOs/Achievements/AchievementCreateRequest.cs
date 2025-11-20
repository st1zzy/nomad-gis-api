namespace nomad_gis_V2.DTOs.Achievements;


public class AchievementCreateRequest
{
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int RewardPoints { get; set; }
    public IFormFile? BadgeFile { get; set; }
}
