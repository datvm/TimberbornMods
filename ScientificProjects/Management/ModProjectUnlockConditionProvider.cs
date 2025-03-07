namespace ScientificProjects.Management;

public class ModProjectUnlockConditionProvider(HazardousWeatherApproachingTimer hazardTimer, ILoc t) : IProjectUnlockConditionProvider
{

    public static readonly ImmutableHashSet<string> EmergencyDrillIds = ["EmergencyDrill1", "EmergencyDrill2", "EmergencyDrill3"];

    public IEnumerable<string> CanCheckUnlockConditionForIds => EmergencyDrillIds;

    public string? CheckForUnlockCondition(ScientificProjectInfo project)
    {
        if (EmergencyDrillIds.Contains(project.Spec.Id))
        {
            return hazardTimer.DaysToHazardousWeather <= HazardousWeatherApproachingTimer.ApproachingNotificationDays 
                ? null :
                "LV.SP.BadWeatherConditionErr".T(t);
        }
        else
        {
            throw new NotSupportedException($"Project {project.Spec.Id} is not supported by this unlock condition provider");
        }
    }
}
