using nomad_gis_V2.Models;

namespace nomad_gis_V2.Interfaces;

public interface IExperienceService
{
    Task<bool> AddExperienceAsync(User user, int amount);
}