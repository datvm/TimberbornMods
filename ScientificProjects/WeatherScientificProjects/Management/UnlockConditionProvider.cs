namespace WeatherScientificProjects.Management;

public class ModProjectUnlockConditionProvider(
    HazardousWeatherApproachingTimer hazardTimer,
    ILoc t
) : IProjectUnlockConditionProvider
{

    public IEnumerable<string> CanCheckUnlockConditionForIds { get; } = WeatherProjectsUtils.EmergencyDrillIds;

    public string? CheckForUnlockCondition(ScientificProjectInfo project)
    {
        if (WeatherProjectsUtils.EmergencyDrillIds.Contains(project.Spec.Id))
        {
            return hazardTimer.GetWeatherStage() == GameWeatherStage.Warning
                ? null :
                "LV.SP.BadWeatherConditionErr".T(t);
        }
        else
        {
            throw new NotSupportedException($"Project {project.Spec.Id} is not supported by this unlock condition provider");
        }
    }
}
