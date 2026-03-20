namespace SkyblightWeather.Services;

[BindSingleton]
public class LandEffectService(IDayNightCycle cycle) : ITickableSingleton, IUnloadableSingleton
{

    public static bool ShouldContaminateLand { get; private set; }
    public static int? LimitMoistureRange { get; private set; }

    int accumulatedTicks = 0;
    int? cleanTicks;
    int? contaminateTicks;
    bool active;

    public void SetLimitMoistureRange(int? range) => LimitMoistureRange = range;
    public void StopLimitingMoisture() => LimitMoistureRange = null;

    public void StartContaminatingLand(float contaminateHours, float cleanHours)
    {
        accumulatedTicks = 0;
        cleanTicks = cycle.HoursToTicks(cleanHours);
        contaminateTicks = cycle.HoursToTicks(contaminateHours);
        ShouldContaminateLand = true;
        active = true;
    }

    public void StopContaminatingLand()
    {
        active = false;
        ShouldContaminateLand = false;        
    }

    public void Tick()
    {
        if (!active) { return; }

        accumulatedTicks++;
        if ((ShouldContaminateLand && accumulatedTicks >= contaminateTicks)
            || (!ShouldContaminateLand && accumulatedTicks >= cleanTicks))
        {
            accumulatedTicks = 0;
            ShouldContaminateLand = !ShouldContaminateLand;
        }
    }

    public void Unload()
    {
        active = false;
        ShouldContaminateLand = false;
        LimitMoistureRange = null;
    }
}
