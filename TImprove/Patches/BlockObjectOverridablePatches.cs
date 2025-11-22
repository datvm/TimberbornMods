namespace TImprove.Patches;

[HarmonyPatch(typeof(BlockObject))]
public static class BlockObjectOverridablePatches
{

    [HarmonyPostfix, HarmonyPatch(nameof(BlockObject.MakeOverridable))]
    public static void AutoRemove(BlockObject __instance)
    {
        if (MSettings.Instance?.AutoClearDeadTrees.Value == true)
        {
            MiscService.Instance?.Delete(__instance);
        }
    }

}
