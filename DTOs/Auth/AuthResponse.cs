namespace nomad_gis_V2.DTOs.Auth;

public class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public UserDto User { get; set; } = new UserDto();
}

public class UserDto
{
    public Guid Id { get; set; } = Guid.Empty;
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public int Experience { get; set; }
    public int Level { get; set; }
    public string? AvatarUrl { get; set; }
}