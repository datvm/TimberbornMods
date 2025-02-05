using TImprove.Services;

namespace TImprove.Patches;

[HarmonyPatch(typeof(DemolishableSelectionTool), "ActionCallback")]
public static class DemolishableSelectionToolPatch
{

    public static void Postfix(IEnumerable<BlockObject> blockObjects)
    {
        if (MSettings.Instance?.AutoClearDeadTrees != true ||
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
