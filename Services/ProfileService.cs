using Amazon.S3;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using nomad_gis_V2.Data;
using nomad_gis_V2.DTOs.Achievements;
using nomad_gis_V2.DTOs.Auth;
using nomad_gis_V2.DTOs.Points;
using nomad_gis_V2.Interfaces;
using nomad_gis_V2.Models;
using nomad_gis_V2.Profile;
using nomad_gis_V2.Exceptions; 
using Amazon.S3.Model;

namespace nomad_gis_V2.Services;

public class ProfileService : IProfileService
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IAmazonS3 _s3Client;
    private readonly ILogger<ProfileService> _logger; 
    private readonly IConfiguration _config;
    private readonly IMapper _mapper;

    public ProfileService(
        ApplicationDbContext context,
        IPasswordHasher<User> passwordHasher,
        IAmazonS3 s3Client,
        ILogger<ProfileService> logger, 
        IConfiguration config,
        IMapper mapper)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _s3Client = s3Client;
        _logger = logger;
        _config = config;
        _mapper = mapper;
    }

    public async Task<UserDto> GetUserProfileAsync(Guid userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            throw new NotFoundException("User not found.");

        return _mapper.Map<UserDto>(user);
    }

    public async Task<List<MapPointRequest>> GetUserPointsAsync(Guid userId)
    {
        var unlockedPoints = await _context.UserMapProgress
            .Where(p => p.UserId == userId)
            .Include(p => p.MapPoint)
            .Select(p => p.MapPoint)
            .ToListAsync();

        return _mapper.Map<List<MapPointRequest>>(unlockedPoints);
    }

    public async Task<List<AchievementResponse>> GetUserAchievementsAsync(Guid userId)
    {
        var userAchievements = await _context.UserAchievements
            .Where(ua => ua.UserId == userId)
            .Include(ua => ua.Achievement)
            .Select(ua => ua.Achievement)
            .ToListAsync();

        return _mapper.Map<List<AchievementResponse>>(userAchievements);
    }

    public async Task<string> UploadAvatarAsync(Guid userId, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw new ValidationException("No file uploaded.");
        }
        
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        var publicUrl = await UploadFileToStorageAsync(file, user.Id.ToString(), user.AvatarUrl);

        user.AvatarUrl = publicUrl;
        await _context.SaveChangesAsync();

        return publicUrl;
    }

    public async Task<UserDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new NotFoundException("User not found.");

        bool hasChanges = false;

        if (!string.IsNullOrEmpty(request.NewPassword))
        {
            if (string.IsNullOrEmpty(request.CurrentPassword))
            {
                throw new ValidationException("Current password is required to set new password.");
            }

            var passResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword);
            if (passResult == PasswordVerificationResult.Failed)
            {
                throw new UnauthorizedException("Invalid current password.");
            }

            user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
            hasChanges = true;
        }

        if (!string.IsNullOrEmpty(request.Username) && user.Username != request.Username)
        {
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                throw new DuplicateException("This username is already taken.");
            }
            user.Username = request.Username;
            hasChanges = true;
        }

        if (request.AvatarFile != null && request.AvatarFile.Length > 0)
        {
            var publicUrl = await UploadFileToStorageAsync(request.AvatarFile, user.Id.ToString(), user.AvatarUrl);
            user.AvatarUrl = publicUrl;
            hasChanges = true;
        }
        
        if (hasChanges)
        {
            await _context.SaveChangesAsync();
        }

        return _mapper.Map<UserDto>(user);
    }
    
    private async Task<string> UploadFileToStorageAsync(IFormFile file, string fileKey, string? oldImageUrl)
    {
        var bucketName = _config["R2Storage:BucketName"];
        var publicUrlBase = _config["R2Storage:PublicUrlBase"];
        var fileExtension = Path.GetExtension(file.FileName);
        var uniqueFileName = $"{fileKey}{fileExtension}";

        try
        {
            if (!string.IsNullOrEmpty(oldImageUrl))
            {
                try
                {
                    var oldKey = Path.GetFileName(new Uri(oldImageUrl).LocalPath);
                    if (oldKey != uniqueFileName)
                    {
                        await _s3Client.DeleteObjectAsync(bucketName, oldKey);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not delete old file from R2: {OldUrl}", oldImageUrl);
                }
            }

            await using var stream = file.OpenReadStream();
            var putRequest = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = uniqueFileName,
                InputStream = stream,
                ContentType = file.ContentType,
                DisablePayloadSigning = true,
                DisableDefaultChecksumValidation = true
            };

            await _s3Client.PutObjectAsync(putRequest);

            return $"{publicUrlBase?.TrimEnd('/')}/{uniqueFileName}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file to R2 for key {FileKey}", fileKey);
            throw new Exception("Error occurred while uploading new avatar.", ex); 
        }
    }
}