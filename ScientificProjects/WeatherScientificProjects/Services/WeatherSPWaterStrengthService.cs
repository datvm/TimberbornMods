namespace WeatherScientificProjects.Services;

public class WeatherSPWaterStrengthService(DefaultEntityTracker<WeatherSPWaterStrengthModifier> tracker) : IScientificProjectDailyListener
{
    public (WeatherSPWaterStrengthInfo Fresh, WeatherSPWaterStrengthInfo Bad) CurrentInfo { get; private set; }
    public FrozenSet<string> ListenerIds { get; } = [
        ..WeatherProjectsUtils.FreshWaterStrengthIds,
        ..WeatherProjectsUtils.BadWaterStrengthIds
    ];

    void SetWaterStrength(IReadOnlyList<ScientificProjectInfo> activeProjects)
    {
        var fresh = GetStrength(activeProjects, true);
        var bad = GetStrength(activeProjects, false);
        CurrentInfo = (fresh, bad);

        foreach (var e in tracker.Entities)
        {
            e.SetModifier(e.IsBadwaterSource ? bad : fresh);
        }
    }

    WeatherSPWaterStrengthInfo GetStrength(IReadOnlyList<ScientificProjectInfo> activeProjects, bool fresh)
    {
        if (activeProjects.Count == 0) { return WeatherSPWaterStrengthInfo.Default; }

        var modifier = 0f;
        var list = (fresh ?
            activeProjects.Where(q => WeatherProjectsUtils.FreshWaterStrengthIds.Contains(q.Spec.Id)) :
            activeProjects.Where(q => WeatherProjectsUtils.BadWaterStrengthIds.Contains(q.Spec.Id)))
            .ToArray();

        if (list.Length > 0)
        {
            foreach (var p in list)
            {
                modifier += p.GetEffect(0);
            }
        }
        
        return new(list, modifier);
    }

    public void OnDailyPaymentResolved(IReadOnlyList<ScientificProjectInfo> activeProjects)
        => SetWaterStrength(activeProjects);

    public void OnListenerLoaded(IReadOnlyList<ScientificProjectInfo> activeProjects) 
        => SetWaterStrength(activeProjects);

}

public readonly record struct WeatherSPWaterStrengthInfo(ScientificProjectInfo[] Projects, float Modifier)
{
    public static readonly WeatherSPWaterStrengthInfo Default = new([], 0f);
}