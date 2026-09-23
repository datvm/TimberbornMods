namespace PowerOverdrive.Patches;

[HarmonyPatch(typeof(Manufactory))]
static class ManufactoryProductionEfficiencyPatch
{
    [HarmonyPostfix, HarmonyPatch(nameof(Manufactory.ProductionEfficiency))]
    public static void MultiplyOverdrive(Manufactory __instance, ref float __result)
    {
        if (__result <= 0f)
        {
            return;
        }

        var overdrive = __instance.GetComponent<PowerOverdriveComponent>();
        if (overdrive)
        {
            __result *= overdrive.ProductionMultiplier;
        }
    }
}
