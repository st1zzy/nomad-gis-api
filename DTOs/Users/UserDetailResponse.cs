using System;
using System.Collections.Generic;

namespace nomad_gis_V2.DTOs.Users;

// Полная информация о пользователе для админки
public class UserDetailResponse
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    public string? AvatarUrl { get; set; }
    
    // Доп. статистика
    public int Level { get; set; }
    public int Experience { get; set; }

    // Связанные данные
    public List<UserPointProgressDto> UnlockedPoints { get; set; } = new();
    public List<UserAchProgressDto> Achievements { get; set; } = new();
}

// Краткая инфо об открытой точке
public class UserPointProgressDto
{
    public Guid MapPointId { get; set; }
    public string MapPointName { get; set; } = string.Empty;
    public DateTime UnlockedAt { get; set; }
}

// Краткая инфо о достижении пользователя
public class UserAchProgressDto
{
    public Guid AchievementId { get; set; }
    public string AchievementTitle { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
}
