namespace ConveyorBelt.Services;

[BindSingleton]
public class BeltSimulator(BeltRegistry registry, IDayNightCycle dayNight) : ITickableSingleton, ILoadableSingleton
{
    public float HoursPerTick { get; private set; }

    public void Load() => HoursPerTick = dayNight.TicksToHours(1);

    public void Tick()
    {
        var carriers = registry.MoveOrder;
        for (var i = 0; i < carriers.Count; i++)
        {
            carriers[i].MoveForward(HoursPerTick);
        }

        for (var i = 0; i < carriers.Count; i++)
        {
            carriers[i].PullFromBuilding();
        }

        var teleporters = registry.Teleporters;
        for (var i = 0; i < teleporters.Count; i++)
        {
            teleporters[i].Teleport();
        }
    }
}
