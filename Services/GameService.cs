using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using nomad_gis_V2.Data;
using nomad_gis_V2.DTOs.Achievements;
using nomad_gis_V2.DTOs.Game;
using nomad_gis_V2.Interfaces;
using nomad_gis_V2.Models;
using nomad_gis_V2.Helpers;
using nomad_gis_V2.DTOs.Auth;

namespace nomad_gis_V2.Services;

public class GameService : IGameService
{
    private readonly ApplicationDbContext _context;
    private readonly IAchievementService _achievementService;
    private readonly IMapper _mapper;
    private readonly GeometryFactory _geometryFactory;
    private readonly IExperienceService _experienceService;
    public GameService(ApplicationDbContext context, IAchievementService achievementService, IMapper mapper, IExperienceService experienceService)
    {
        _context = context;
        _achievementService = achievementService;
        _mapper = mapper;
        _geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        _experienceService = experienceService;
    }
    public async Task<GameEventResponse> CheckAndUnlockPointsAsync(Guid userId, CheckLocationRequest request)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new Exception("User not found");

        var unlockedPointIds = await _context.UserMapProgress
            .Where(p => p.UserId == userId)
            .Select(p => p.MapPointId)
            .ToHashSetAsync();

        var userLocation = _geometryFactory.CreatePoint(new Coordinate(request.Longitude, request.Latitude));

        var potentialPointsToUnlock = await _context.MapPoints
            .Where(p => !unlockedPointIds.Contains(p.Id))
            .Where(p => p.Location.IsWithinDistance(userLocation, p.UnlockRadiusMeters))
            .ToListAsync();

        foreach (var point in potentialPointsToUnlock)
        {
            
            var progress = new UserMapProgress
            {
                UserId = userId,
                MapPointId = point.Id,
                UnlockedAt = DateTime.UtcNow
            };
            _context.UserMapProgress.Add(progress);

            int newUnlockedCount = await _context.UserMapProgress
                .CountAsync(p => p.UserId == userId) + 1;

            var context = new AchievementContext{ TotalPointsUnlocked = newUnlockedCount };

            var newAchievemnts = await _achievementService.CheckAchievementsAsync(
                userId,
                AchievementEvent.PointedUnlocked,
                context
            );

            int expGained = 100;
            int achievementsExp = newAchievemnts.Sum(a => a.RewardPoints);
            int totalExpGained = expGained + achievementsExp;

            bool leveledUp = false;

            if (totalExpGained > 0)
            {
                leveledUp = await _experienceService.AddExperienceAsync(user, totalExpGained);
            }

            await _context.SaveChangesAsync();

            return new GameEventResponse
            {
                Success = true,
                Message = $"Вы открыли точку: {point.Name}!",
                UnlockedPointId = point.Id,
                ExperienceGained = totalExpGained,
                UnlockedAchievements = _mapper.Map<List<AchievementResponse>>(newAchievemnts),
                LeveledUp = leveledUp,
                UserData = new UserDto 
                {
                    Id = user.Id,
                    Email = user.Email,
                    Username = user.Username,
                    Experience = user.Experience,
                    Level = user.Level,          
                    AvatarUrl = user.AvatarUrl
                }
            };
        }

        return new GameEventResponse { Success = false, Message = "Поблизости нет новых точек." };
    }
}