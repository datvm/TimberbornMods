global using Timberborn.HazardousWeatherSystemUI;

namespace WeatherScientificProjects.Management;

public class WaterSourceTracking : ITrackingEntities
{
    public IEnumerable<Type> TrackingTypes => [typeof(WaterSourceContaminationSpec)];
}

public class WeatherProjectsCostProvider(EntityManager entities, HazardousWeatherApproachingTimer hazardTimer) : IProjectCostProvider
{
    public IEnumerable<string> CanCalculateCostForIds => [
        .. WeatherProjectsUtils.FreshWaterStrengthIds,
        .. WeatherProjectsUtils.BadWaterStrengthIds,
        .. WeatherProjectsUtils.PrewarningIds];

    public int CalculateCost(ScientificProjectInfo project)
    {
        var spec = project.Spec;
        var id = spec.Id;

        if (WeatherProjectsUtils.BadWaterStrengthIds.Contains(id))
        {
            return this.LevelOr0(project, l => l * spec.ScienceCost * CountWaterSources(false));
        }
        else if (WeatherProjectsUtils.FreshWaterStrengthIds.Contains(id))
        {
            return this.LevelOr0(project, l => l * spec.ScienceCost * CountWaterSources(true));
        }
        else if (WeatherProjectsUtils.PrewarningIds.Contains(id))
        {
            return CalculatePrewarningCost(project);
        }
        else
        {
            throw spec.ThrowNotSupportedEx();
        }
    }

    int CalculatePrewarningCost(ScientificProjectInfo info)
    {
        return hazardTimer.IsPreWarning(false) ? info.Spec.ScienceCost * info.Level : 0;
    }

    int CountWaterSources(bool fresh)
    {
        var result = entities.Get<WaterSourceContaminationSpec>().AsEnumerable()
            .Count(spec => fresh ? spec.DefaultContamination == 0 : spec.DefaultContamination > 0);

        Debug.Log($"CountWaterSources({fresh}) = {result}");

        return result;
    }

}
