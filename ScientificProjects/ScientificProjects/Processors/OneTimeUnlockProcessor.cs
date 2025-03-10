namespace ScientificProjects.Processors;

public class OneTimeUnlockProcessor(
    EventBus eb,
    ScientificProjectService projects,
    TemperateWeatherDurationService temperateWeatherDurationService
) : ILoadableSingleton
{
    public const string WheelbarrowsId = "Wheelbarrows";

    public event Action OnWheelbarrowsUnlocked = delegate { };
    public bool HasWheelbarrows { get; private set; }

    public void Load()
    {
        CheckForWheelbarrows();

        eb.Register(this);
    }

    [OnEvent]
    public void OnProjectUnlocked(OnScientificProjectUnlockedEvent ev)
    {
        var id = ev.Project.Id;

        switch (id)
        {
            case WheelbarrowsId:
                InternalWheelbarrowsUnlocked();
                break;
            default:
                if (ModProjectUnlockConditionProvider.EmergencyDrillIds.Contains(id))
                {
                    ExtendGoodWeatherDays(ev.Project);
                }

                break;
        }
    }

    void ExtendGoodWeatherDays(ScientificProjectSpec spec)
    {
        temperateWeatherDurationService.TemperateWeatherDuration += (int)spec.Parameters[0];
    }

    void InternalWheelbarrowsUnlocked()
    {
        HasWheelbarrows = true;
        OnWheelbarrowsUnlocked();
    }

    void CheckForWheelbarrows()
    {
        HasWheelbarrows = projects.IsUnlocked(WheelbarrowsId);
    }
    
}
