namespace ConfigurableFaction.Patches;

[HarmonyPatch(typeof(AutoAtlaser))]
public static class AutoAtlaserPatches
{

    [HarmonyPrefix, HarmonyPatch(nameof(AutoAtlaser.TooManyAtlases))]
    public static bool IgnoreTooManyAtlases(ref bool __result)
    {
        __result = false;
        return false;
    }

}
