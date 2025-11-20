using nomad_gis_V2.DTOs.Game;
using nomad_gis_V2.DTOs.Messages;

namespace nomad_gis_V2.Interfaces;

public interface IMessageService
{
    Task<GameEventResponse> CreateMessageAsync(Guid userId, MessageRequest request);
    Task<IEnumerable<MessageResponse>> GetMessagesByPointIdAsync(Guid mapPointId, Guid currentUserId);
    Task<bool> DeleteMessageAsync(Guid messageId, Guid userId);
    Task<bool> AdminDeleteMessageAsync(Guid messageId);

    Task<GameEventResponse> ToggleLikeAsync(Guid messageId, Guid userId);

}

