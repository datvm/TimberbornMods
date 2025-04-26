namespace TheArchitectsToolkit.Patches;

[HarmonyPatch]
public static class WaterSourceStrengthPatches
{

    [HarmonyPostfix, HarmonyPatch(typeof(BlockObjectFactory), nameof(BlockObjectFactory.CreateFinished))]
    public static void SetWaterSourceStrength(BlockObject __result)
    {
        var ws = __result.GetComponentFast<WaterSource>();
        if (ws)
        {
            ws.SpecifiedStrength = MSettings.DefaultWaterSourceStrength;
        }
    }

}
