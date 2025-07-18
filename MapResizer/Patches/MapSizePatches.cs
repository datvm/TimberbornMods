﻿namespace MapResizer.Patches;

[HarmonyPatch]
public static class MapSizePatches
{

    static readonly PropertyKey<Vector2Int> MapSizeHeightKey = new("MapHeight");
    static WeakReference<MapSize>? mapSizeRef;

    static readonly FieldInfo MaxMapEditorTerrainHeight = typeof(MapSize).Field(nameof(MapSize.MaxMapEditorTerrainHeight));
    [HarmonyPostfix, HarmonyPatch(typeof(MapSize), nameof(MapSize.Initialize))]
    public static void LoadHeight(MapSize __instance, Vector2Int size)
    {
        if (__instance._singletonLoader is null
            || !__instance._singletonLoader.TryGetSingleton(MapSize.MapSizeKey, out var s)
            || !s.Has(MapSizeHeightKey))
        {
            return;
        }

        var height = s.Get(MapSizeHeightKey);
        __instance.TerrainSize = new Vector3Int(size.x, size.y, height.x);
        __instance.TotalSize = new Vector3Int(size.x, size.y, height.y);

        MaxMapEditorTerrainHeight.SetValue(null, height.x - 1);
        Debug.Log($"{nameof(MapResizer)}: Map size changed: {__instance.TerrainSize} - {__instance.TotalSize}");

        mapSizeRef = new(__instance);
    }

    [HarmonyPostfix, HarmonyPatch(typeof(MapSize), nameof(MapSize.Save))]
    public static void SaveHeight(MapSize __instance, ISingletonSaver singletonSaver)
    {
        singletonSaver.GetSingleton(MapSize.MapSizeKey)
            .Set(MapSizeHeightKey, new(__instance.TerrainSize.z, __instance.TotalSize.z));
    }

    [HarmonyPrefix, HarmonyPatch(typeof(TerrainLevelPreviewsValidator), nameof(TerrainLevelPreviewsValidator.IsTopTerrainConstraintDissatisfied))]
    public static bool CheckShouldUseMapSize(Preview preview, ref bool __result)
    {
        if (mapSizeRef is null || !mapSizeRef.TryGetTarget(out var mapSize)) { return true; }

        if (preview.GetComponentFast<TopTerrainLevelValidationConstraint>())
        {
            __result = preview.BlockObject.Coordinates.z >= mapSize.TerrainSize.z - 1;
        }
        else
        {
            __result = false;
        }

        return false;
    }

    [HarmonyPostfix, HarmonyPatch(typeof(WorldTiling), nameof(WorldTiling.TileCount3D))]
    public static void UpdateHeightCount(ref Vector3Int __result)
    {
        if (mapSizeRef is null || !mapSizeRef.TryGetTarget(out var mapSize)) { return; }
        __result = __result with { z = WorldTiling.VerticalTileCount(mapSize.TerrainSize.z), };
    }



    [HarmonyPrefix, HarmonyPatch(typeof(LevelVisibilityService), nameof(LevelVisibilityService.Load))]
    public static void SetVisibilityServiceMaxVisibleLevel(LevelVisibilityService __instance)
    {
        __instance.MaxVisibleLevel = __instance._mapSize.TerrainSize.z - 1;
    }

}
