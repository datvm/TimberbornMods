namespace ConfigurablePumps;

public class MechPumpPowerModifier : BaseComponent
{

    public void Start()
    {
        var comp = GetComponentFast<MechanicalNode>();
        if (!comp)
        {
            Debug.Log($"Didn't find {nameof(MechanicalNode)}. This should not be right");
            return;
        }

        var defaultRatio = comp._nominalPowerInput / (SpecPatches.OriginalMechPumpWater ?? 0.25f);
        var newPowerInput = MSettings.MechPumpWater * defaultRatio;
        var actualInput = newPowerInput * MSettings.MechPumpPowerMultiplier;

        comp._nominalPowerInput = (int)actualInput;
    }

}