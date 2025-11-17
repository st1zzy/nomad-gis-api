namespace nomad_gis_V2.DTOs.Auth;

public class LoginRequest
{
    public string Identifier { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
