namespace WeatherScientificProjects.Services;

public class WeatherSPCostProvider(
    ModdableWeatherApproachingTimer weatherTimer,
    DefaultEntityTracker<WeatherSPWaterStrengthModifier> waterSourceTracker
) : IProjectCostProvider
{
    public IEnumerable<string> CanCalculateCostForIds => [
        .. WeatherProjectsUtils.FreshWaterStrengthIds,
        .. WeatherProjectsUtils.BadWaterStrengthIds,
        .. WeatherProjectsUtils.PrewarningIds];

    public int CalculateCost(ScientificProjectSpec spec, int level)
    {
        var id = spec.Id;

        if (WeatherProjectsUtils.BadWaterStrengthIds.Contains(id))
        {
            return this.LevelOr0(level, l => l * spec.ScienceCost * CountWaterSources(false));
        }
        else if (WeatherProjectsUtils.FreshWaterStrengthIds.Contains(id))
        {
            return this.LevelOr0(level, l => l * spec.ScienceCost * CountWaterSources(true));
        }
        else if (WeatherProjectsUtils.PrewarningIds.Contains(id))
        {
            return CalculatePrewarningCost(spec, level);
        }
        else
        {
            throw spec.ThrowNotSupportedEx();
        }
    }

    int CalculatePrewarningCost(ScientificProjectSpec spec, int level) 
        => weatherTimer.GetNextWeatherWarningProgress() < 0f ? spec.ScienceCost * level : 0;

    int CountWaterSources(bool fresh)
    {
        var result = waterSourceTracker.Entities
            .Count(src =>
            {
                if (src.CurrentStrength == 0) { return false; } // Don't count if it's being disabled somehow (drought, sealed)
                return fresh != src.IsBadwaterSource;
            });

        return result;
    }

}
