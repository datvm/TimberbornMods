namespace MechanicalFilterPump.Patches;

[HarmonyPatch]
public static class WaterFilterPatches
{

    [HarmonyPrefix, HarmonyPatch(typeof(WaterOutput), nameof(WaterOutput.AddWater))]
    public static void PurifyWaterPatch(ref float cleanWater, ref float contaminatedWater, WaterOutput __instance)
    {
        var comp = __instance.GetComponentFast<MechanicalFilterPumpComponent>();
        if (!comp || !comp.IsActive || contaminatedWater <= 0) { return; }

        cleanWater += contaminatedWater;
        contaminatedWater = 0;
    }

}
