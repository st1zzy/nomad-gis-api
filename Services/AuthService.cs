using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using nomad_gis_V2.Data;
using nomad_gis_V2.DTOs.Auth;
using nomad_gis_V2.Interfaces;
using nomad_gis_V2.Models;
using nomad_gis_V2.Exceptions;
using Amazon.S3; // <-- 1. ПОДКЛЮЧАЕМ ИСКЛЮЧЕНИЯ

namespace nomad_gis_V2.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly JwtService _jwtService;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IAmazonS3 _s3Client;
    private readonly IConfiguration _config;

    public AuthService(ApplicationDbContext context, JwtService jwtService, IPasswordHasher<User> passwordHasher, IAmazonS3 s3Client, IConfiguration config)
    {
        _context = context;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
        _s3Client = s3Client;
        _config = config;
        
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, HttpContext httpContext)
    {
        // 2. Генерируем кастомные исключения
        if (request.Password != request.Password)
        {
            throw new ValidationException("Passwords do not match"); // <-- ИЗМЕНЕНО
        }

        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            throw new DuplicateException("User with this email already exists"); // <-- ИЗМЕНЕНО
        }

        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
        {
            throw new DuplicateException("User with this username already exists"); // <-- ИЗМЕНЕНО
        }

        var httpReq = httpContext.Request;
        var baseUrl = _config["R2Storage:PublicUrlBase"];
        
        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            IsActive = true,
            Role = "User", // <-- ДОБАВЛЕНО: Явно указываем роль при регистрации
            AvatarUrl = $"{baseUrl?.TrimEnd('/')}/default.jpg"
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var (accessToken, refreshToken) = _jwtService.GenerateTokens(user);

        var rt = new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id,
            DeviceId = request.DeviceId,
            Expires = DateTime.UtcNow.AddDays(7) // TODO: вынести в конфиг
        };

        _context.RefreshTokens.Add(rt);
        await _context.SaveChangesAsync();

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                Experience = user.Experience,
                Level = user.Level
            }
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Identifier || u.Username == request.Identifier);

        // 2. Генерируем кастомные исключения
        if (user == null)
        {
            throw new UnauthorizedException("Invalid email or password"); // <-- ИЗМЕНЕНО
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedException("Invalid email or password"); // <-- ИЗМЕНЕНО
        }

        var (accessToken, refreshToken) = _jwtService.GenerateTokens(user);

        // ... (код сохранения RefreshToken)
        var rt = new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id,
            DeviceId = request.DeviceId,
            Expires = DateTime.UtcNow.AddDays(7) 
        };
        _context.RefreshTokens.Add(rt);
        await _context.SaveChangesAsync();


        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                AvatarUrl = user.AvatarUrl,
                Experience = user.Experience,
                Level = user.Level
            }
        };
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var refreshToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

        // 2. Генерируем кастомные исключения
        if (refreshToken == null)
        {
            throw new UnauthorizedException("Invalid refresh token"); // <-- ИЗМЕНЕНО
        }

        if (refreshToken.RevorkedAt.HasValue || refreshToken.Expires < DateTime.UtcNow)
        {
            throw new UnauthorizedException("Refresh token is expired or revoked"); // <-- ИЗМЕНЕНО
        }

        var user = refreshToken.User;
        var (newAccessToken, newRefreshToken) = _jwtService.GenerateTokens(user);

        // ... (код обновления токена)
        refreshToken.Token = newRefreshToken;
        refreshToken.Expires = DateTime.UtcNow.AddDays(7); 
        
        _context.RefreshTokens.Update(refreshToken);
        await _context.SaveChangesAsync();

        return new AuthResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                AvatarUrl = user.AvatarUrl,
                Experience = user.Experience,
                Level = user.Level
            }
        };
    }

    public async Task<bool> LogoutAsync(LogoutRequest request)
    {
        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken && rt.DeviceId == request.DeviceId);

        if (refreshToken == null)
        {
            // 2. Генерируем кастомные исключения
            // Раньше ты возвращал false и контроллер выдавал 404,
            // теперь сервис сразу сообщает об ошибке 404.
            throw new NotFoundException("Refresh token not found"); // <-- ИЗМЕНЕНО
        }

        _context.RefreshTokens.Remove(refreshToken);
        await _context.SaveChangesAsync();
        
        return true;
    }
}