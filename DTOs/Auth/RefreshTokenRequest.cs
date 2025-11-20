namespace nomad_gis_V2.DTOs.Auth;

public class RefreshTokenRequest
{
    public Guid UserId { get; set; } = Guid.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
}