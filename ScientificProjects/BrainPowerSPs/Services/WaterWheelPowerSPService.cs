namespace BrainPowerSPs.Services;

public class WaterWheelPowerSPService(
    DefaultEntityTracker<WaterWheelPowerSPComponent> tracker,
    IDayNightCycle dayNightCycle,
    IThreadSafeWaterMap waterMap
) : DefaultProjectUpgradeListener, ILoadableSingleton, ITickableSingleton
{

    public ScientificProjectInfo? ActivePowerUpProject { get; private set; }
    public IReadOnlyList<ScientificProjectInfo> ActiveFlowUpProjects { get; private set; } = [];

    public bool IsDynamicBoost { get; private set; }
    public bool IsFlowUpActive { get; private set; }
    public float CurrentModifier { get; private set; } = 1f;
    public float MinimumModifier { get; private set; } = 0f;

    public override FrozenSet<string> UnlockListenerIds { get; } = [.. PowerProjectsUtils.WaterWheelUpIds, PowerProjectsUtils.WaterWheelFlowUp1Id];
    public override FrozenSet<string> DailyListenerIds { get; } = [PowerProjectsUtils.WaterWheelFlowUp2Id];

    public override void Load()
    {
        base.Load();

        tracker.OnEntityRegistered += OnEntityRegistered;
    }

    void OnEntityRegistered(WaterWheelPowerSPComponent obj)
    {
        if (ActivePowerUpProject is not null)
        {
            obj.OnModifierChanged();
        }
    }

    protected override void ProcessActiveProjects(IReadOnlyList<ScientificProjectInfo> activeProjects, ScientificProjectSpec? newUnlock, ActiveProjectsSource source)
    {
        var isUnlockingPowerUp = source == ActiveProjectsSource.Unlock && PowerProjectsUtils.WaterWheelUpIds.Contains(newUnlock!.Id);

        var shouldNotify = false;
        if (source == ActiveProjectsSource.Load || isUnlockingPowerUp)
        {
            if (RefreshPowerUpUpgrades(activeProjects))
            {
                shouldNotify = true;
            }
        }

        if (!isUnlockingPowerUp)
        {
            if (RefreshFlowUpgrades(activeProjects))
            {
                shouldNotify = true;
            }
        }

        if (shouldNotify)
        {
            foreach (var e in tracker.Entities)
            {
                e.OnModifierChanged();
            }
        }
    }

    bool RefreshPowerUpUpgrades(IReadOnlyList<ScientificProjectInfo> activeProjects)
    {
        ActivePowerUpProject = GetBestWaterWheelUp(activeProjects);
        IsDynamicBoost = ActivePowerUpProject is not null && ActivePowerUpProject.Id != PowerProjectsUtils.WaterWheelUp3Id;

        if (ActivePowerUpProject is not null)
        {
            CurrentModifier = GetCurrentBoost();
            return true;
        }

        return false;
    }

    bool RefreshFlowUpgrades(IReadOnlyList<ScientificProjectInfo> activeProjects)
    {
        ActiveFlowUpProjects = [.. activeProjects
            .Where(q => q.Id == PowerProjectsUtils.WaterWheelFlowUp1Id || q.Id == PowerProjectsUtils.WaterWheelFlowUp2Id)];
        IsFlowUpActive = ActiveFlowUpProjects.Count > 0;

        var newModifier = GetCurrentMinimum();
        if (newModifier == MinimumModifier) { return false; }

        MinimumModifier = newModifier;
        return true;
    }

    ScientificProjectInfo? GetBestWaterWheelUp(IReadOnlyList<ScientificProjectInfo> activeProjects)
        => activeProjects
            .Where(q => PowerProjectsUtils.WaterWheelUpIds.Contains(q.Id))
            .OrderByDescending(q => q.Id)
            .FirstOrDefault();

    public void Tick()
    {
        UpdateDayTimeBoost();
        UpdateMinimumStrength();
    }

    void UpdateDayTimeBoost()
    {
        if (!IsDynamicBoost) { return; }

        CurrentModifier = GetCurrentBoost();
        foreach (var e in tracker.Entities)
        {
            e.OnModifierChanged();
        }
    }

    void UpdateMinimumStrength()
    {
        if (!IsFlowUpActive || MinimumModifier == 0) { return; }

        var min = MinimumModifier;
        foreach (var e in tracker.Entities)
        {
            e.MinimumGeneratorStrength = e.GroundedCoordinates.FastAny(waterMap.CellIsUnderwater) 
                ? min 
                : 0f;
        }
    }

    float GetCurrentBoost()
    {
        if (ActivePowerUpProject is null) { return 1; }

        var spec = ActivePowerUpProject.Spec;
        if (spec.Id == PowerProjectsUtils.WaterWheelUp3Id)
        {
            return 1 + spec.Parameters[0];
        }

        var dayProgress = dayNightCycle.DayProgress;
        return 1 + Mathf.Lerp(spec.Parameters[0], spec.Parameters[1], dayProgress);
    }

    float GetCurrentMinimum() => ActiveFlowUpProjects.Count == 0 ? 0f : ActiveFlowUpProjects.Sum(q => q.GetEffect(0));

}
