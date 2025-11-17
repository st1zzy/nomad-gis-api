using nomad_gis_V2.DTOs.Achievements;
using nomad_gis_V2.DTOs.Auth;
using nomad_gis_V2.DTOs.Points;
using nomad_gis_V2.Profile;

namespace nomad_gis_V2.Interfaces;

public interface IProfileService
{
    Task<UserDto> GetUserProfileAsync(Guid userId);
    Task<List<MapPointRequest>> GetUserPointsAsync(Guid userId);
    Task<List<AchievementResponse>> GetUserAchievementsAsync(Guid userId);
    Task<string> UploadAvatarAsync(Guid userId, IFormFile file);
    Task<UserDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request);
}