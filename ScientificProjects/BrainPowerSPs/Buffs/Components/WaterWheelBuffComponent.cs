global using Timberborn.TickSystem;

namespace BrainPowerSPs.Buffs.Components;

public class WaterWheelBuffComponent : TickableComponent
{
    WaterPoweredGenerator generator = null!;

    public bool HasWater { get; private set; }

    public void Awake()
    {
        generator = GetComponentFast<WaterPoweredGenerator>();
        if (!generator)
        {
            throw new InvalidOperationException($"{nameof(WaterWheelBuffComponent)} requires a {nameof(WaterPoweredGenerator)} component.");
        }
    }

    void CheckForWater()
    {
        HasWater = generator._groundedCoordinates
            .FastAny(generator._threadSafeWaterMap.CellIsUnderwater);
    }

    public override void Tick()
    {
        CheckForWater();
    }
}
