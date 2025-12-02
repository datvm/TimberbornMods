namespace HydroFormaProjects.Patches;

[HarmonyPatch]
public static class SluiceAutoPatches
{
    readonly static ConditionalWeakTable<Sluice, SluiceUpstreamComponent> upstreamComps = [];

    [HarmonyPrefix, HarmonyPatch(typeof(Sluice), nameof(Sluice.UpdateOutflowLimit))]
    public static bool UpdateOutflowLimit(Sluice __instance)
    {
        if (!upstreamComps.TryGetValue(__instance, out var upstream))
        {
            upstream = __instance.GetComponent<SluiceUpstreamComponent>();
            if (!upstream) { return true; }

            upstreamComps.Add(__instance, upstream);
        }
        if (!upstream.AutoOpen) { return true; }

        var upstreamHeight = __instance._threadSafeWaterMap.WaterHeightOrFloor(upstream.ThresholdCoordinates);

        var isIncreasingFlow = __instance._flowControllerState == FlowControllerState.IncreaseFlow;
        var increasingThreshold = upstream.Threshold + upstream.ThresholdCoordinates.z - (isIncreasingFlow ? Sluice.SluiceOverflowLimit : 0);
        
        if (upstreamHeight >= increasingThreshold)
        {
            if (!isIncreasingFlow)
            {
                __instance._waterService.SetControllerToIncreaseFlow(__instance._blockObject.Coordinates);
                __instance._flowControllerState = FlowControllerState.IncreaseFlow;
            }

            return false;
        }

        return true; // Let the original logic decide to open or close
    }

}
