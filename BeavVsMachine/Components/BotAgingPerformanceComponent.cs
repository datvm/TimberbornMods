namespace BeavVsMachine.Components;

public class BotAgingPerformanceComponent : BaseComponent, IEntityEffectDescriber
{
    public const string BotPerformanceBonusId = "BotDeteriorityPerformance";
    public const float MinWorkPerformance = .2f;
    public const float MaxWorkPerformance = 1f;
    public const float MinSpeedPerformance = .5f;
    public const float MaxSpeedPerformance = 1f;

#nullable disable
    Deteriorable deteriorable;
    BonusTrackerComponent bonusTracker;
#nullable enable

    float progress, workPerf, speedPerf;

    public int Order { get; }

    public void Awake()
    {
        deteriorable = GetComponentFast<Deteriorable>();
        bonusTracker = GetComponentFast<BonusTrackerComponent>();
    }

    public void Start()
    {
        UpdatePerformance();
    }

    public void UpdatePerformance()
    {
        progress = deteriorable.DeteriorationProgress; // From 1 to 0

        workPerf = Mathf.Lerp(MinWorkPerformance, MaxWorkPerformance, progress) - 1f;
        speedPerf = Mathf.Lerp(MinSpeedPerformance, MaxSpeedPerformance, progress) - 1f;
        bonusTracker.AddOrUpdate(new(BotPerformanceBonusId, [
            BonusType.WorkingSpeed.ToBonusSpec(workPerf),
            BonusType.MovementSpeed.ToBonusSpec(speedPerf)
        ]));
    }

    public EntityEffectDescription? Describe(ILoc t, IDayNightCycle dayNightCycle)
        => new(
            t.T("LV.BVM.DeteriorationTitle", progress),
            t.T("LV.BVM.DeteriorationDesc", workPerf, speedPerf)
        );
}
