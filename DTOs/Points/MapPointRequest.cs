namespace nomad_gis_V2.DTOs.Points;

public class MapPointRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double UnlockRadiusMeters { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
