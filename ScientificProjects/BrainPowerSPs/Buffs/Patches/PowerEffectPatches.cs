global using Timberborn.MechanicalSystem;

namespace BrainPowerSPs.Buffs.Patches;

[HarmonyPatch]
public static class PowerEffectPatches
{

    [HarmonyPrefix, HarmonyPatch(typeof(MechanicalNode), nameof(MechanicalNode.UpdateOutput))]
    public static bool PatchUpdateOutput(float output, MechanicalNode __instance)
    {
        var buffable = __instance.GetBuffable();
        var flatPower = 0f;

        foreach (var b in buffable.Buffs)
        {
            if (!b.Active) { continue; }

            foreach (var eff in b.Effects)
            {
                switch (eff)
                {
                    case PowerOutputDayMultiplierBuffEff podm:
                        output += podm.CurrentValue;
                        break;
                    case PowerOutputMultiplierBuffEff pom:
                        output += pom.Value;
                        break;
                    case PowerOutputFlatBuffEff pof:
                        flatPower += pof.Value;
                        break;
                }
            }
        }

        var g = __instance.Graph;
        g.DeactivateNode(__instance);
        __instance.PowerOutput = (int)(flatPower + __instance._nominalPowerOutput * output);
        g.ActivateNode(__instance);

        return false;
    }

}
