namespace ConveyorBelt.Services;

[BindSingleton]
public class BeltSimulator(BeltRegistry registry, IDayNightCycle dayNight) : ITickableSingleton, ILoadableSingleton
{
    public float HoursPerTick { get; private set; }

    public void Load() => HoursPerTick = dayNight.TicksToHours(1);

    public void Tick()
    {
        registry.Prepare();
        registry.Simulation.Tick(HoursPerTick);
        registry.PublishStuck();
    }
}
