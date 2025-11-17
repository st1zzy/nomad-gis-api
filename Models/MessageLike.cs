using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace nomad_gis_V2.Models;

[PrimaryKey(nameof(UserId), nameof(MessageId))]
public class MessageLike()
{
    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;
    public Guid MessageId { get; set; }
    public virtual Message Message { get; set; } = null!;

    public DateTime LikedAt { get; set; } = DateTime.UtcNow;
}