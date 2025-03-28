global using Timberborn.TickSystem;

namespace BrainPowerSPs.Buffs.Components;

public class WaterWheelBuffComponent : TickableComponent
{
    BuffableComponent buffable = null!;
    WaterPoweredGenerator generator = null!;
    
    public void Awake()
    {
        buffable = this.GetBuffable();
        generator = GetComponentFast<WaterPoweredGenerator>()
            ?? throw new InvalidOperationException($"{nameof(WaterWheelBuffComponent)} requires a {nameof(WaterPoweredGenerator)} component.");
    }

    void CheckForWater()
    {
        var nowHasWater = generator._groundedCoordinates
            .FastAny(generator._threadSafeWaterMap.CellIsUnderwater);

        foreach (var b in buffable.Buffs)
        {
            if (b is not WaterWheelFlowUpBuffInst wwb) { continue; }

            var effs = b.GetBuffEffects<GeneratorMinStrengthBuffEff>();
            foreach (var eff in effs)
            {
                eff.Enabled = nowHasWater;
            }
        }
    }

    public override void Tick()
    {
        CheckForWater();
    }
}
