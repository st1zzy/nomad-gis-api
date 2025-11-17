using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nomad_gis_V2.Data;
using System.Threading.Tasks; // Убедитесь, что это добавлено
using System; // Убедитесь, что это добавлено

namespace nomad_gis_V2.Controllers;

[ApiController]
[Route("api/v1/dashboard")]
[Authorize(Roles = "Admin")] // Только для админов
public class DashboardController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var totalUsers = await _context.Users.CountAsync();
        var totalPoints = await _context.MapPoints.CountAsync();
        var totalMessages = await _context.Messages.CountAsync();
        var totalUnlocks = await _context.UserMapProgress.CountAsync();
        var totalAchievementsWon = await _context.UserAchievements.CountAsync(ua => ua.IsCompleted);

        var newUsersToday = await _context.Users
            .CountAsync(u => u.CreatedAt > DateTime.UtcNow.AddDays(-1));
        
        var newMessagesToday = await _context.Messages
            .CountAsync(m => m.CreatedAt > DateTime.UtcNow.AddDays(-1));

        var stats = new
        {
            TotalUsers = totalUsers,
            TotalMapPoints = totalPoints,
            TotalMessages = totalMessages,
            TotalUnlocks = totalUnlocks,
            TotalAchievementsWon = totalAchievementsWon,
            NewUsersToday = newUsersToday,
            NewMessagesToday = newMessagesToday
        };

        return Ok(stats);
    }
}
