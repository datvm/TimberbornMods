using TImprove.Settings;

namespace TImprove.Patches;

[HarmonyPatch]
public static class DemolishableSelectionToolPatch
{

    [HarmonyPostfix, HarmonyPatch(typeof(BlockObject), nameof(BlockObject.MakeOverridable))]
    public static void AutoRemove(BlockObject __instance)
    {
        if (MSettings.Instance?.ClearDeadStumpValue != ClearDeadStumpModeValue.Auto ||
            GameDepServices.Instance is null) { return; }

        GameDepServices.Instance.DeleteObject(__instance);
    }

    [HarmonyPostfix, HarmonyPatch(typeof(DemolishableSelectionTool), "ActionCallback")]
    public static void RemoveOnToolUse(IEnumerable<BlockObject> blockObjects)
    {
        if (MSettings.Instance?.ClearDeadStumpValue == ClearDeadStumpModeValue.No ||
            GameDepServices.Instance is null) { return; }

        foreach (var o in blockObjects)
        {
            if (o.Overridable)
            {
                GameDepServices.Instance.DeleteObject(o);
            }
        }
    }

}
