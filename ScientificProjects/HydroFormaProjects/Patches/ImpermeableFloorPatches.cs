namespace HydroFormaProjects.Patches;

[HarmonyPatch]
public static class ImpermeableFloorPatches
{
    
    [HarmonyPostfix, HarmonyPatch(typeof(TerrainPhysicsPostLoader), nameof(TerrainPhysicsPostLoader.GetInitialCandidates))]
    public static void AddFloorsToCandidates(TerrainPhysicsPostLoader __instance)
    {
        foreach (var e in __instance._entityRegistry.Entities)
        {
            var template = e.GetComponent<TemplateSpec>();
            if (template?.TemplateName.StartsWith(" =ImpermeableFloor.") != true) { continue; }

            var block = e.GetComponent<BlockObject>();
            __instance._candidates.Enqueue(new(block.Coordinates, 0));
        }
    }

}
