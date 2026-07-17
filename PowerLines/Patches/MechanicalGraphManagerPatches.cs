namespace PowerLines.Patches;

[HarmonyPatch(typeof(MechanicalGraphManager))]
public static class MechanicalGraphManagerPatches
{

    [HarmonyPostfix, HarmonyPatch(nameof(MechanicalGraphManager.AddNode))]
    public static void OnAfterAddNode(MechanicalNode mechanicalNode)
    {
        PowerLineGraphService.Instance?.OnNodeAddedToGraph(mechanicalNode);
    }
}
