namespace MapTransformer.Patches;

[HarmonyPatch]
static class MapSizePatches
{

    /// <summary>
    /// MapResizer wrote <c>MapHeight</c> on the <see cref="MapSize"/> singleton as
    /// <c>(TerrainSize.z, TotalSize.z)</c>. Keep this key so those maps still load.
    /// </summary>
    static readonly PropertyKey<Vector2Int> MapSizeHeightKey = new("MapHeight");

    [HarmonyPostfix, HarmonyPatch(typeof(MapSize), nameof(MapSize.Load))]
    static void LoadHeight(MapSize __instance)
    {
        if (!TryGetSavedHeight(__instance, out var terrainZ, out var totalZ))
        {
            return;
        }

        var maxTerrainHeight = terrainZ - 1;
        var maxHeightAboveTerrain = totalZ - terrainZ;

        __instance._mapSizeSpec = __instance._mapSizeSpec with
        {
            MaxGameTerrainHeight = maxTerrainHeight,
            MaxMapEditorTerrainHeight = maxTerrainHeight,
            MaxHeightAboveTerrain = maxHeightAboveTerrain,
        };

        __instance.Initialize(__instance.TerrainSize2D);

        TimberUiUtils.LogVerbose(() => $"[{nameof(MapTransformer)}] Loaded height: Terrain = {terrainZ}, Total = {totalZ}, Map Size: {__instance.TerrainSize2D}");
    }

    [HarmonyPostfix, HarmonyPatch(typeof(MapSize), nameof(MapSize.Save))]
    static void SaveHeight(MapSize __instance, ISingletonSaver singletonSaver)
    {
        if (__instance.TerrainSize.z == 0)
        {
            return;
        }

        singletonSaver.GetSingleton(MapSize.MapSizeKey)
            .Set(MapSizeHeightKey, new(__instance.TerrainSize.z, __instance.TotalSize.z));
    }

    static bool TryGetSavedHeight(MapSize mapSize, out int terrainZ, out int totalZ)
    {
        terrainZ = 0;
        totalZ = 0;

        if (mapSize._singletonLoader is null
            || !mapSize._singletonLoader.TryGetSingleton(MapSize.MapSizeKey, out var s)
            || !s.Has(MapSizeHeightKey))
        {
            return false;
        }

        (terrainZ, totalZ) = s.Get(MapSizeHeightKey);
        return true;
    }

}
