namespace ConveyorBelt.Services;

[BindSingleton]
public class BeltSimulator(BeltRegistry registry, IDayNightCycle dayNight) : ITickableSingleton, ILoadableSingleton
{
    public float HoursPerTick { get; private set; }
    int generation = 1;

    public void Load() => HoursPerTick = dayNight.TicksToHours(1);

    public void Tick()
    {
        var carriers = registry.OrderForTick();
        generation++;
        if (generation == 0)
        {
            generation = 1;
        }

        for (var i = 0; i < carriers.Count; i++)
        {
            carriers[i].BeginTick(generation);
        }

        for (var i = 0; i < carriers.Count; i++)
        {
            carriers[i].MoveForward(HoursPerTick);
        }

        for (var i = 0; i < carriers.Count; i++)
        {
            carriers[i].PullFromBuilding(HoursPerTick);
        }
    }
}
