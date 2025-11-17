using System.ComponentModel.DataAnnotations;

namespace nomad_gis_V2.DTOs.Points;

public class MapPointUpdateRequest
    {
        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(-90, 90)]
        public double Latitude { get; set; }

        [Required]
        [Range(-180, 180)]
        public double Longitude { get; set; }

        [Required]
        [Range(1, double.MaxValue)]
        public double UnlockRadiusMeters { get; set; }

        public string? Description { get; set; }
    }
