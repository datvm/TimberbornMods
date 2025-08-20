namespace HydroFormaProjects.Patches;

[HarmonyPatch]
public static class ImpermeableFloorPatches
{
    
    [HarmonyPostfix, HarmonyPatch(typeof(TerrainPhysicsPostLoader), nameof(TerrainPhysicsPostLoader.GetInitialCandidates))]
    public static void AddFloorsToCandidates(TerrainPhysicsPostLoader __instance)
    {
        foreach (var e in __instance._entityRegistry.Entities)
        {
            var prefab = e.GetComponentFast<PrefabSpec>();
            if (!prefab || !prefab.PrefabName.StartsWith("ImpermeableFloor.")) { continue; }

            var block = e.GetComponentFast<BlockObject>();
            __instance._candidates.Enqueue(new(block.Coordinates, 0));
        }
    }

}
