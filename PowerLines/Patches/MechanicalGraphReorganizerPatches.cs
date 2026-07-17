namespace PowerLines.Patches;

[HarmonyPatch(typeof(MechanicalGraphReorganizer))]
public static class MechanicalGraphReorganizerPatches
{

    [HarmonyPostfix, HarmonyPatch("VisitConnectedNodes")]
    public static void OnAfterVisitConnectedNodes(MechanicalGraphReorganizer __instance, MechanicalNode node)
    {
        if (PowerLineGraphService.Instance is not { } service) { return; }

        var oldGraphNodes = __instance._oldGraphNodes;
        var currentGraphNodes = __instance._currentGraphNodes;

        foreach (var linked in service.GetPowerLineLinkedNodes(node))
        {
            if (oldGraphNodes.Contains(linked))
            {
                currentGraphNodes.Add(linked);
            }
        }
    }
}
