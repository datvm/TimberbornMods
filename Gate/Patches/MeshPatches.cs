namespace Gate.Patches;

[HarmonyPatch(typeof(BuildingNavMesh))]
public static class MeshPatches
{

    [HarmonyPrefix, HarmonyPatch(nameof(BuildingNavMesh.OnExitFinishedState))]
    public static bool DontRemoveClosedGate(BlockObjectNavMeshAdder __instance)
    {
        var gate = __instance.GetComponentFast<GateComponent>();
        return !gate || !gate.Closed;
    }

}
