namespace HydroFormaProjects.Services;

public class FloodgateAutoService(
    ScientificProjectService projects,
    EventBus eb,
    EntityManager entities
) : BaseProjectService(projects), ILoadableSingleton
{

    protected override string ProjectId { get; } = HydroFormaModUtils.FloodgateUpgrade;

    public void Load()
    {
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
        if (!CanUseProject) { return; }

        foreach (var f in entities.Get<FloodgateAutoComponent>())
        {
            if (!f) { continue; }

            action(f);
        }
    }
}
