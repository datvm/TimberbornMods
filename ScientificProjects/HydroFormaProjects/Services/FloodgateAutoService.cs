namespace HydroFormaProjects.Services;

public class FloodgateAutoService(
    DefaultEntityTracker<FloodgateAutoComponent> tracker,
    EventBus eb
) : SimpleProjectListener
{

    public override string ProjectId { get; } = HydroFormaModUtils.FloodgateUpgrade;

    public override void Load()
    {
        base.Load();
        eb.Register(this);
    }

    [OnEvent]
    public void OnHazardousWeather(HazardousWeatherStartedEvent _) => PerformFloodgateAction(f =>
    {
        if (!f.SetOnHazard) { return; }
        f.SetHeight(f.HeightOnHazard);
    });

    [OnEvent]
    public void OnNewCycle(CycleStartedEvent _) => PerformFloodgateAction(f =>
    {
        if (!f.SetOnNewCycle) { return; }
        f.SetHeight(f.HeightOnNewCycle);
    });

    void PerformFloodgateAction(Action<FloodgateAutoComponent> action)
    {
        if (!IsUnlocked) { return; }

        foreach (var f in tracker.Entities)
        {
            action(f);
        }
    }
}
