namespace nomad_gis_V2.DTOs.Auth;

public class LogoutRequest
{
    public Guid UserId { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
}