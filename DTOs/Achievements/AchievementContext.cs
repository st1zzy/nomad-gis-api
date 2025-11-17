namespace nomad_gis_V2.DTOs.Achievements;

public class AchievementContext
{
    public int TotalPointsUnlocked { get; set; } = 0;
    public int TotalMessagesPosted { get; set; } = 0;
    public int TotalMessagesLiked { get; set; } = 0;
    public int LikesOnThisMessage { get; set; } = 0;
}