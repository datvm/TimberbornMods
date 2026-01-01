namespace WeatherScientificProjects.Services;

public class EmergencyDrillService(
    WeatherHistoryRegistry historyRegistry,
    WeatherCycleService weatherCycle
) : IScientificProjectUpgradeListener
{
    public FrozenSet<string> UnlockListenerIds { get; } = WeatherProjectsUtils.EmergencyDrillIds;
    public FrozenSet<string> ListenerIds { get; } = []; // Not needed

    public void OnDailyPaymentResolved(IReadOnlyList<ScientificProjectInfo> activeProjects) { }

    public void OnListenerLoaded(IReadOnlyList<ScientificProjectInfo> activeProjects) { }

    public void OnProjectUnlocked(ScientificProjectSpec project, IReadOnlyList<ScientificProjectInfo> activeProjects)
    {
        ExtendTemperateWeather(project);
    }

    void ExtendTemperateWeather(ScientificProjectSpec project)
    {
        var days = Mathf.RoundToInt(project.Parameters[0]);

        var currStage = weatherCycle.CurrentStage;
        var currCycle = currStage.CycleIndex;
        
        var list = (List<WeatherCycle>)historyRegistry.WeatherCycles;

        var stages = list[currCycle].Stages.ToArray();
        var stageHistory = stages[currStage.StageIndex];
        stages[currStage.StageIndex] = stageHistory with
        {
            Days = stageHistory.Days + days,
        };

        list[currCycle] = list[currCycle] with
        {
            Stages = [..stages],
        };
    }

}
