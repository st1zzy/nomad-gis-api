using System.ComponentModel.DataAnnotations;

namespace nomad_gis_V2.DTOs.Game;

public class CheckLocationRequest
{
    [Required, Range(-90, 90)]
    public double Latitude { get; set; }

    [Required, Range(-180, 180)]
    public double Longitude { get; set; }
}