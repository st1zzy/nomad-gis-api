using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NetTopologySuite.Geometries;

namespace nomad_gis_V2.Models;

public class MapPoint
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "geography(Point, 4326)")]
    public Point Location { get; set; } = null!;

    [Required]
    public double UnlockRadiusMeters { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual ICollection<UserMapProgress> UserProgress { get; set; } = new List<UserMapProgress>();
}
