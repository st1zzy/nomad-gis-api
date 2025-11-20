namespace nomad_gis_V2.Helpers;

public static class LevelCalculator
{
    public static int GetRequiredExperience(int currentLevel)
    {
        return currentLevel * 100;
    }
}