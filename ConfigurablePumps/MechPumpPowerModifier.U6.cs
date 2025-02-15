#if TIMBER6
using ConfigurablePumps.Patches;
using Timberborn.MechanicalSystem;
using Timberborn.WaterBuildings;

namespace ConfigurablePumps;

public class MechPumpPowerModifier : BaseComponent
{
    float? originalWaterPerS;

    public void Start()
    {
        var mover = GetComponentFast<WaterMover>();
        if (!mover)
        {
            Debug.Log($"Didn't find {nameof(WaterMover)}. This should not be right");
            return;
        }
        originalWaterPerS ??= mover._waterPerSecond;
        mover._waterPerSecond = MSettings.MechPumpWater;

        var comp = GetComponentFast<MechanicalNode>();
        if (!comp)
        {
            Debug.Log($"Didn't find {nameof(MechanicalNode)}. This should not be right");
            return;
        }

        var defaultRatio = comp._nominalPowerInput / originalWaterPerS;
        var newPowerInput = MSettings.MechPumpWater * defaultRatio;
        var actualInput = newPowerInput * MSettings.MechPumpPowerMultiplier;

        comp._nominalPowerInput = (int)actualInput;
    }

}
#endif