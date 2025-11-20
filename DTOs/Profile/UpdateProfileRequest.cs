namespace nomad_gis_V2.Profile;

public class UpdateProfileRequest
{
    public string? Username { get; set; }
    public string? CurrentPassword { get; set; }
    public string? NewPassword { get; set; }
    public IFormFile? AvatarFile { get; set; }
}