
namespace GateV1.Patches;

[HarmonyPatch(typeof(BuildingNavMesh))]
public static class MeshPatches
{

    [HarmonyPrefix, HarmonyPatch(nameof(BuildingNavMesh.OnExitFinishedState))]
    public static bool DontRemoveClosedGate(BlockObjectNavMeshAdder __instance)
    {
        var gate = __instance.GetComponent<GateComponent>();
        return !gate || !gate.Closed;
    }

}
