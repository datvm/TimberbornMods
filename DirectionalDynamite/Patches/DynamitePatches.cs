namespace DirectionalDynamite.Patches;

[HarmonyPatch(typeof(Dynamite))]
public static class DynamitePatches
{

    [HarmonyPrefix, HarmonyPatch(nameof(Dynamite.LowerTerrainBelow))]
    public static bool DestroyDirectionalTerrainPatch(Dynamite __instance)
    {
        var directional = __instance.GetComponentFast<DirectionalDynamiteComponent>();
        return directional ? directional.DestroyTerrain() : true;
    }

    [HarmonyPrefix, HarmonyPatch(nameof(Dynamite.TriggerNeighbors))]
    public static bool TriggerDirectionalNeighborsPatch(Dynamite __instance)
    {
        var directional = __instance.GetComponentFast<DirectionalDynamiteComponent>();
        return directional && (!directional.DoNotTriggerNeighbor);
    }


}
