using nomad_gis_V2.Helpers;
using nomad_gis_V2.Interfaces;
using nomad_gis_V2.Models;

namespace nomad_gis_V2.Services;

public class ExperienceService : IExperienceService
{
    public Task<bool> AddExperienceAsync(User user, int amount)
    {
        if (user == null || amount <= 0)
        {
            return Task.FromResult(false);
        }

        user.Experience += amount;

        bool leveledUp = false;

        int requiredXp = LevelCalculator.GetRequiredExperience(user.Level);

        while (user.Experience >= requiredXp)
        {
            user.Level += 1;
            user.Experience -= requiredXp;
            leveledUp = true;

            requiredXp = LevelCalculator.GetRequiredExperience(user.Level);
        }

        return Task.FromResult(leveledUp);
    }
}