using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nomad_gis_V2.DTOs.Auth;
using nomad_gis_V2.DTOs.Points;
using nomad_gis_V2.DTOs.Achievements;
using nomad_gis_V2.Profile;
using nomad_gis_V2.Interfaces;

namespace nomad_gis_V2.Controllers;

[ApiController]
[Route("api/v1/profile")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;


    public ProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> UserProfile()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _profileService.GetUserProfileAsync(userId);

        return Ok(user);
    }

    [HttpGet("my-points")]
    public async Task<ActionResult<List<MapPointRequest>>> UserPoints()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var progress = await _profileService.GetUserPointsAsync(userId);

        return Ok(progress);
    }

    [HttpGet("my-achievements")]
    public async Task<ActionResult<List<AchievementResponse>>> UserAchievemnts()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var userAchievements = await _profileService.GetUserAchievementsAsync(userId);

        return Ok(userAchievements);
    }

    [HttpPost("avatar")]
    [RequestSizeLimit(5_000_000)] // Ограничение 5MB
    public async Task<IActionResult> UploadAvatar(IFormFile file)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var publicUrl = await _profileService.UploadAvatarAsync(userId, file);

        return Ok(new { avatarUrl = publicUrl });
    }

    [HttpPut("me")]
    [RequestSizeLimit(5_000_000)]
    public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var updatedUser = await _profileService.UpdateProfileAsync(userId, request);

        return Ok(updatedUser);
    }
}