namespace TImprove4Ui.Patches;

[HarmonyPatch(typeof(NeedApplicationUIConfigurator))]
public static class NeedApplicationUIConfiguratorPatches
{

    [HarmonyPrefix, HarmonyPatch(nameof(NeedApplicationUIConfigurator.Configure))]
    public static bool Bypass() => !MSettings.AddNegativeNeedsValue;

}
