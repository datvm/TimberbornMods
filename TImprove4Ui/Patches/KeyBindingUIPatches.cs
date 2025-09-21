namespace TImprove4Ui.Patches;

[HarmonyPatch]
public static class KeyBindingUIPatches
{

    [HarmonyPrefix, HarmonyPatch(typeof(KeyBindingGroupSpec), nameof(KeyBindingGroupSpec.IsHiddenGroup), MethodType.Getter)]
    public static bool PatchHiddenGroup(ref bool __result)
    {
        __result = false;
        return false;
    }

}
