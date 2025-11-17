using nomad_gis_V2.DTOs.Achievements;
using nomad_gis_V2.DTOs.Auth;
using nomad_gis_V2.DTOs.Messages;

namespace nomad_gis_V2.DTOs.Game;
public class GameEventResponse
{
    public bool Success { get; set; } = false;
    public string Message { get; set; } = string.Empty;
    public int ExperienceGained { get; set; } = 0;
    public bool LeveledUp { get; set; } = false;
    public UserDto? UserData { get; set; }
    public List<AchievementResponse> UnlockedAchievements { get; set; } = new();

    public Guid? UnlockedPointId { get; set; }
    public MessageResponse? CreatedMessage { get; set; }
    public bool? IsLiked { get; set; }
}
