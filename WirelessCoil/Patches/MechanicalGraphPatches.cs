namespace WirelessCoil.Patches;

[HarmonyPatch]
public static class MechanicalGraphPatches
{

    [HarmonyPostfix, HarmonyPatch(typeof(MechanicalGraphReorganizer), nameof(MechanicalGraphReorganizer.Reorganize))]
    public static void ConnectWirelessGraph()
    {
        WirelessCoilService.Instance?.ConnectGraphs();
    }

}
