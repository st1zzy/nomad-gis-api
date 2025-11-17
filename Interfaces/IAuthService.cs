using nomad_gis_V2.DTOs.Auth;

namespace nomad_gis_V2.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RegisterAsync(RegisterRequest request, HttpContext httpContext);
    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);
    Task<bool> LogoutAsync(LogoutRequest request);
}
