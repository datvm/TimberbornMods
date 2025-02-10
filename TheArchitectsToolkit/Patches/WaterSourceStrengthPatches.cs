using Timberborn.BlockSystem;
using Timberborn.WaterSourceSystem;

namespace TheArchitectsToolkit.Patches;

[HarmonyPatch]
public static class WaterSourceStrengthPatches
{

    [HarmonyPostfix, HarmonyPatch(typeof(BlockObjectFactory), nameof(BlockObjectFactory.CreateFinished))]
    public static void SetWaterSourceStrength(BlockObject __result)
    {
        Debug.Log($"Something placed");

        var ws = __result.GetComponentFast<WaterSource>();
        if (ws)
        {
            Debug.Log($"WS placed");
            ws.SpecifiedStrength = MSettings.DefaultWaterSourceStrength;
        }
    }

}
