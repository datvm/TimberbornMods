namespace MapResizer.Patches;

[HarmonyPatch]
public static class MapSizePatches
{

    static readonly PropertyKey<Vector2Int> MapSizeHeightKey = new("MapHeight");

    public static int? maxGameTerrainHeight, maxMapEditorTerrainHeight, maxHeightAboveTerrain;

    static bool ReplaceValueIfAvailable(int? value, ref int __result)
    {
        if (value is not null)
        {
            __result = value.Value;
            return false;
        }
        return true;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(MapSizeSpec), nameof(MapSizeSpec.MaxGameTerrainHeight), MethodType.Getter)]
    public static bool ModifyMaxGameTerrainHeight(ref int __result)
        => ReplaceValueIfAvailable(maxGameTerrainHeight, ref __result);

    [HarmonyPrefix, HarmonyPatch(typeof(MapSizeSpec), nameof(MapSizeSpec.MaxMapEditorTerrainHeight), MethodType.Getter)]
    public static bool ModifyMaxMapEditorTerrainHeight(ref int __result)
        => ReplaceValueIfAvailable(maxMapEditorTerrainHeight, ref __result);

    [HarmonyPrefix, HarmonyPatch(typeof(MapSizeSpec), nameof(MapSizeSpec.MaxHeightAboveTerrain), MethodType.Getter)]
    public static bool ModifyMaxHeightAboveTerrain(ref int __result)
        => ReplaceValueIfAvailable(maxHeightAboveTerrain, ref __result);

    [HarmonyPrefix, HarmonyPatch(typeof(MapSize), nameof(MapSize.Initialize))]
    public static void LoadHeight(MapSize __instance, Vector2Int size)
    {
        maxGameTerrainHeight = null;
        maxMapEditorTerrainHeight = null;
        maxHeightAboveTerrain = null;

        if (__instance._singletonLoader is null
            || !__instance._singletonLoader.TryGetSingleton(MapSize.MapSizeKey, out var s)
            || !s.Has(MapSizeHeightKey))
        {
            return;
        }

        var (terrainZ, totalZ) = s.Get(MapSizeHeightKey);

        maxMapEditorTerrainHeight = maxGameTerrainHeight = terrainZ - 1;
        maxHeightAboveTerrain = totalZ - terrainZ;

        Debug.Log($"[{nameof(MapResizer)}] Loaded height: Terrain = {terrainZ}, Total = {totalZ}, Map Size: {size}");
    }

    [HarmonyPostfix, HarmonyPatch(typeof(MapSize), nameof(MapSize.Save))]
    public static void SaveHeight(MapSize __instance, ISingletonSaver singletonSaver)
    {
        singletonSaver.GetSingleton(MapSize.MapSizeKey)
            .Set(MapSizeHeightKey, new(__instance.TerrainSize.z, __instance.TotalSize.z));
    }

}
