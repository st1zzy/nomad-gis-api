namespace nomad_gis_V2.DTOs.Leaderboard;

public class LeaderboardEntryDto
{
    public int Rank { get; set; }
    public Guid UserId { get; set; }
    public string? AvatarUrl { get; set; }
    public string Username { get; set; } = string.Empty;
    public int Level { get; set; }
    public int Score { get; set; }
}