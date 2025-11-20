using Microsoft.EntityFrameworkCore;

namespace nomad_gis_V2.Models;

[PrimaryKey(nameof(UserId), nameof(MapPointId))]
public class UserMapProgress
{
    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;

    public Guid MapPointId { get; set; }
    public virtual MapPoint MapPoint { get; set; } = null!;

    public DateTime UnlockedAt { get; set; } = DateTime.UtcNow;
}
