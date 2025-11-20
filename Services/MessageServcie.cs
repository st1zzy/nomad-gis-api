using Microsoft.EntityFrameworkCore;
using nomad_gis_V2.Data;
using nomad_gis_V2.DTOs.Achievements;
using nomad_gis_V2.DTOs.Auth;
using nomad_gis_V2.DTOs.Game;
using nomad_gis_V2.DTOs.Messages;
using nomad_gis_V2.Helpers;
using nomad_gis_V2.Interfaces;
using nomad_gis_V2.Models;
using AutoMapper;

namespace nomad_gis_V2.Services
{
    public class MessageService : IMessageService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAchievementService _achievementService;
        private readonly IExperienceService _experienceService;
        private readonly IMapper _mapper;

        public MessageService(
            ApplicationDbContext context,
            IAchievementService achievementService,
            IExperienceService experienceService,
            IMapper mapper)
        {
            _context = context;
            _achievementService = achievementService;
            _experienceService = experienceService;
            _mapper = mapper;
        }

        public async Task<GameEventResponse> CreateMessageAsync(Guid userId, MessageRequest request)
        {
            var user = await _context.Users.FindAsync(userId)
                ?? throw new Exception("Пользователь не найден.");

            var point = await _context.MapPoints.FindAsync(request.MapPointId)
                ?? throw new Exception("Точка не найдена.");

            var message = new Message
            {
                Content = request.Content,
                UserId = userId,
                MapPointId = request.MapPointId
            };
            _context.Messages.Add(message);

            var newMessagesCount = await _context.Messages
                .CountAsync(m => m.UserId == userId);

            var context = new AchievementContext { TotalMessagesPosted = newMessagesCount + 1};

            var newAchievements = await _achievementService.CheckAchievementsAsync(
                userId,
                AchievementEvent.MessagePosted,
                context
            );

            int achievementsExp = newAchievements.Sum(a => a.RewardPoints);
            int totalExpGained = achievementsExp;

            bool leveledUp = false;
            
            if (totalExpGained > 0)
            {
                leveledUp = await _experienceService.AddExperienceAsync(user, totalExpGained);
            }

            await _context.SaveChangesAsync();

            return new GameEventResponse
            {
                Success = true,
                Message = "Сообщение опубликовано!",
                ExperienceGained = totalExpGained,
                LeveledUp = leveledUp,
                UnlockedAchievements = _mapper.Map<List<AchievementResponse>>(newAchievements),
                UserData = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Username = user.Username,
                    Experience = user.Experience,
                    Level = user.Level,
                    AvatarUrl = user.AvatarUrl
                },
                CreatedMessage = new MessageResponse()
                {
                    Id = message.Id,
                    Content = message.Content,
                    CreatedAt = message.CreatedAt,
                    UserId = user.Id,
                    Username = user.Username,
                    MapPointId = message.MapPointId
                }
            };
        }

        public async Task<IEnumerable<MessageResponse>> GetMessagesByPointIdAsync(Guid mapPointId, Guid currentUserId)
        {
            var messages = await _context.Messages
                .Where(m => m.MapPointId == mapPointId)
                .Include(m => m.User)
                .Include(m => m.Likes)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            return messages.Select(m => new MessageResponse
            {
                Id = m.Id,
                Content = m.Content,
                CreatedAt = m.CreatedAt,
                UserId = m.UserId,
                Username = m.User.Username,
                MapPointId = m.MapPointId,
                AvatarUrl = m.User.AvatarUrl,
                LikesCount = m.Likes.Count,
                IsLikedByCurrentUser = m.Likes.Any(l => l.UserId == currentUserId)
            });
        }

        public async Task<bool> AdminDeleteMessageAsync(Guid messageId)
        {
            var message = await _context.Messages.FindAsync(messageId);
            if (message == null)
            {
                return false;
            }

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteMessageAsync(Guid messageId, Guid userId)
        {
            var message = await _context.Messages.FindAsync(messageId);
            if (message == null || message.UserId != userId)
            {
                return false;
            }

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<GameEventResponse> ToggleLikeAsync(Guid messageId, Guid userId)
        {
            var message = await _context.Messages
                .Include(m => m.Likes)
                .FirstOrDefaultAsync(m => m.Id == messageId);

            if (message == null) throw new Exception("Message not found!");

            var likerUser = await _context.Users.FindAsync(userId);
            if (likerUser == null) throw new Exception("Liker not found");

            var authorUserId = message.UserId;
            var existingLike = message.Likes.FirstOrDefault(l => l.UserId == userId);

            if (existingLike != null)
            {
                _context.MessageLikes.Remove(existingLike);
                await _context.SaveChangesAsync();
                
                return new GameEventResponse 
                { 
                    Success = true, 
                    IsLiked = false,
                    Message = "Лайк снят"
                };
            }

            int likerTotalLikes = await _context.MessageLikes.CountAsync(l => l.UserId == userId);
            var likerContext = new AchievementContext { TotalMessagesLiked = likerTotalLikes + 1 };
            var likerAchievements = await _achievementService.CheckAchievementsAsync(
                userId, AchievementEvent.MessageLiked, likerContext
            );

            _context.MessageLikes.Add(new MessageLike { UserId = userId, MessageId = messageId });

            List<Achievement> authorAchievements = new();
            if (userId != authorUserId)
            {
                int newLikesOnMessage = message.Likes.Count(l => l.UserId != authorUserId) + 1;
                var authorContext = new AchievementContext { LikesOnThisMessage = newLikesOnMessage };
                
                authorAchievements = await _achievementService.CheckAchievementsAsync(
                    authorUserId, AchievementEvent.MessageLikeReceived, authorContext
                );
            }

            int achievementsExp = likerAchievements.Sum(a => a.RewardPoints);
            int totalExpGained = achievementsExp; 

            bool leveledUp = false;
            
            if (totalExpGained > 0)
            {
                leveledUp = await _experienceService.AddExperienceAsync(likerUser, totalExpGained);
            }
            
            await _context.SaveChangesAsync();

            return new GameEventResponse
            {
                Success = true,
                Message = "Лайк добавлен!",
                IsLiked = true,
                ExperienceGained = totalExpGained,
                LeveledUp = leveledUp,
                UnlockedAchievements = _mapper.Map<List<AchievementResponse>>(likerAchievements),
                UserData = new UserDto
                {
                    Id = likerUser.Id,
                    Email = likerUser.Email,
                    Username = likerUser.Username,
                    Experience = likerUser.Experience,
                    Level = likerUser.Level,
                    AvatarUrl = likerUser.AvatarUrl
                }
            };
        }
    }
}
