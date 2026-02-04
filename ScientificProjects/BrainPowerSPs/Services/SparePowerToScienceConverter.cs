
namespace BrainPowerSPs.Services;

public class SparePowerToScienceConverter(
    MechanicalGraphRegistry graphs,
    ScienceService sciences,
    ISingletonLoader loader,
    IDayNightCycle dayNightCycle
) : IScientificProjectUnlockListener, ITickableSingleton, ILoadableSingleton, ISaveableSingleton
{
    static readonly SingletonKey SaveKey = new("BrainPower.ScienceConverter");
    static readonly PropertyKey<string> AccumulationKey = new("Accumulation");
    static readonly PropertyKey<int> AccumulationCountKey = new("AccumulationCount");
    static readonly PropertyKey<float> TimePassedKey = new("TimePassed");

    float gameHourTime;
    long powerAccumulation;
    int accumulationCount;
    float timePassed;
    float conversionRate;

    public FrozenSet<string> UnlockListenerIds { get; } = [PowerProjectsUtils.SparePowerScienceId];
    public FrozenSet<string> ListenerIds { get; } = [PowerProjectsUtils.SparePowerScienceId];

    public bool Unlocked { get; private set; }
    public long AccumulatedScience => accumulationCount == 0 ? 0 : Mathf.FloorToInt(powerAccumulation / conversionRate / accumulationCount);
    public float RemainingHour => 1f - (timePassed / gameHourTime);

    void Unlock(ScientificProjectSpec? project)
    {
        if (project is null || project.Id != PowerProjectsUtils.SparePowerScienceId) { return; }

        conversionRate = project.Parameters[0];
        Unlocked = true;
    }

    public void OnListenerLoaded(IReadOnlyList<ScientificProjectInfo> activeProjects)
    {
        if (activeProjects.Count > 0)
        {
            Unlock(activeProjects[0].Spec);
        }
    }

    public void OnProjectUnlocked(ScientificProjectSpec project, IReadOnlyList<ScientificProjectInfo> activeProjects)
        => Unlock(project);

    public void Tick()
    {
        if (!Unlocked) { return; }

        foreach (var p in graphs.MechanicalGraphs)
        {
            if (p.PowerSupply > p.PowerDemand)
            {
                powerAccumulation += p.PowerSupply - p.PowerDemand;
            }
        }
        accumulationCount++;

        timePassed += Time.fixedDeltaTime;
        if (timePassed >= gameHourTime)
        {
            var accumulated = AccumulatedScience;

            timePassed = 0;
            powerAccumulation = 0;
            accumulationCount = 0;

            if (accumulated >= 1)
            {
                sciences.AddPoints((int)accumulated);
            }
        }
    }

    public void Load()
    {
        gameHourTime = dayNightCycle.DayLengthInSeconds / 24f;
        LoadSavedData();
    }

    void LoadSavedData()
    {
        if (!loader.TryGetSingleton(SaveKey, out var s)) { return; }

        if (s.Has(AccumulationKey))
        {
            powerAccumulation = long.Parse(s.Get(AccumulationKey));
        }

        if (s.Has(AccumulationCountKey))
        {
            accumulationCount = s.Get(AccumulationCountKey);
        }

        if (s.Has(TimePassedKey))
        {
            timePassed = s.Get(TimePassedKey);
        }
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        if (!Unlocked) { return; }

        var s = singletonSaver.GetSingleton(SaveKey);

        s.Set(AccumulationKey, powerAccumulation.ToString());
        s.Set(AccumulationCountKey, accumulationCount);
        s.Set(TimePassedKey, timePassed);
    }
}
