namespace TImprove4Ui.Patches;

[HarmonyPatch]
public static class MechHighlighterPatches
{

    [HarmonyPrefix, HarmonyPatch(typeof(MechanicalNodeSelfMarkerDrawer), nameof(MechanicalNodeSelfMarkerDrawer.Enable))]
    public static bool EnableAllKinds(MechanicalNodeSelfMarkerDrawer __instance)
    {
        if (!MSettings.HighlightPowerNetwork) { return true; }

        __instance.enabled = __instance._transputSpecsProviderSpec;
        return false;
    }

}
