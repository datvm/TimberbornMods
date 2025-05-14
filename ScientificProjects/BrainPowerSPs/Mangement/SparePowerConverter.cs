namespace BrainPowerSPs.Mangement;

public class SparePowerConverter(
    IDayNightCycle time,
    ISingletonLoader loader,
    EventBus eb,
    ScientificProjectService projects,
    MechanicalGraphRegistry graphs,
    ScienceService sciences
) : ILoadableSingleton, ISaveableSingleton, ITickableSingleton
{
    static readonly SingletonKey SaveKey = new("BrainPower.ScienceConverter");
    static readonly PropertyKey<string> AccumulationKey = new("Accumulation");
    static readonly PropertyKey<int> AccumulationCountKey = new("AccumulationCount");
    static readonly PropertyKey<float> TimePassedKey = new("TimePassed");

    float hourLength;
    long powerAccumulation;
    int accumulationCount;
    float timePassed;
    float conversionRate;

    bool unlocked;

    public void Load()
    {
        hourLength = time.ConfiguredDayLengthInSeconds / (time.DaytimeLengthInHours + time.NighttimeLengthInHours);
        Debug.Log("Hour Length: " + hourLength);

        LoadSavedData();
        CheckForUnlock();
    }

    void CheckForUnlock()
    {
        var project = projects.GetProject(ModUtils.SparePowerScienceId);
        conversionRate = project.Spec.Parameters[0];

        if (project.Unlocked)
        {
            unlocked = true;
        }
        else
        {
            eb.Register(this);
        }
    }

    [OnEvent]
    public void OnProjectUnlocked(OnScientificProjectUnlockedEvent ev)
    {
        if (ev.Project.Id == ModUtils.SparePowerScienceId)
        {
            unlocked = true;
            eb.Unregister(this);
        }
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
        var s = singletonSaver.GetSingleton(SaveKey);

        s.Set(AccumulationKey, powerAccumulation.ToString());
        s.Set(AccumulationCountKey, accumulationCount);
        s.Set(TimePassedKey, timePassed);
    }

    public void Tick()
    {
        if (!unlocked) { return; }

        foreach (var g in graphs.MechanicalGraphs)
        {
            var p = g.CurrentPower;
            if (p.PowerSupply > p.PowerDemand)
            {
                powerAccumulation += p.PowerSupply - p.PowerDemand;
            }
        }
        accumulationCount++;

        timePassed += Time.fixedDeltaTime;
        if (timePassed > hourLength)
        {
            var accumulated = powerAccumulation / conversionRate / accumulationCount;

            timePassed = 0;
            powerAccumulation = 0;
            accumulationCount = 0;

            if (accumulated >= 1)
            {
                sciences.AddPoints((int) accumulated);
            }
        }
    }

}
