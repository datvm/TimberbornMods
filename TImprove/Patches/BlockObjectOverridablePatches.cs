namespace TImprove.Patches;

[HarmonyPatch(typeof(BlockObject))]
public static class BlockObjectOverridablePatches
{

    [HarmonyPostfix, HarmonyPatch(nameof(BlockObject.MakeOverridable))]
    public static void AutoRemove(BlockObject __instance)
    {
        if ((MSettings.Instance?.AutoClearDeadTrees.Value) != true) { return; }

        var comp = __instance.GetComponent<LivingNaturalResource>();
        if (!comp || !comp.IsDead) { return; }

        MiscService.Instance?.Delete(__instance);
    }

}
