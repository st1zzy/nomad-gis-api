using nomad_gis_V2.DTOs.Points;
using nomad_gis_V2.Models;

namespace nomad_gis_V2.Interfaces
{
    public interface IMapPointService
    {
        Task<List<MapPointRequest>> GetAllAsync();
        Task<MapPointRequest?> GetByIdAsync(Guid id);
        Task<MapPointRequest> CreateAsync(MapPointCreateRequest dto);
        Task<MapPointRequest?> UpdateAsync(Guid id, MapPointUpdateRequest dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
