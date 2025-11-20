using nomad_gis_V2.DTOs.Game;

namespace nomad_gis_V2.Interfaces;

public interface IGameService
{
    Task<GameEventResponse> CheckAndUnlockPointsAsync(Guid userId, CheckLocationRequest request);
}
