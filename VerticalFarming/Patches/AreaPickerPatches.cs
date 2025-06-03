using static Timberborn.AreaSelectionSystem.AreaPicker;

namespace VerticalFarming.Patches;

[HarmonyPatch]
public static class AreaPickerPatches
{

    // Let the tool to work without ground
    [HarmonyPrefix, HarmonyPatch(typeof(AreaPicker), nameof(AreaPicker.PickTerrainIntArea))]
    public static bool PatchPickTerrainIntArea(IntAreaCallback previewCallback, IntAreaCallback actionCallback, Action showNoneCallback, AreaPicker __instance, ref bool __result)
    {
        if (!MSettings.WithoutGround) { return true; }

        __result = __instance._areaSelectionController.ProcessInput(
            (start, end, _) =>
            {
                previewCallback(ModTerrainService.ModifiedGetTerrainBlocks(start, end, __instance), start);
            },
            (start, end, _) =>
            {
                var blocks = ModTerrainService.ModifiedGetTerrainBlocks(start, end, __instance);
                actionCallback(blocks, start);
            },
            showNoneCallback
        );

        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(TerrainAreaService), nameof(TerrainAreaService.InMapLeveledCoordinates))]
    public static bool PatchInMapLeveledCoordinates(IEnumerable<Vector3Int> inputBlocks, ref IEnumerable<Vector3Int> __result)
    {
        if (!MSettings.WithoutGround) { return true; }

        __result = inputBlocks;
        return false;
    }

    // Bring Soil Moisture and Contamination up
    [HarmonyPostfix, HarmonyPatch(typeof(SoilMoistureService), nameof(SoilMoistureService.SoilIsMoist))]
    public static void PatchSoilIsMoist(Vector3Int coordinates, SoilMoistureService __instance, ref bool __result)
    {
        PatchSoilIs(
            ref __result,
            coordinates,
            __instance._mapIndexService,
            __instance._threadSafeColumnTerrainMap,
            __instance.SoilIsMoist);
    }

    [HarmonyPrefix, HarmonyPatch(typeof(SoilMoistureMap), nameof(SoilMoistureMap.SetMoistureLevel))]
    public static void PatchSetMoistureLevel(SoilMoistureMap __instance, Vector3Int coordinates, int index3D, float newLevel)
    {
        PatchSetSoilLevel(__instance, coordinates, newLevel,
            i => i._moistureLevels[index3D],
            i => i._terrainService,
            (i, c) => i.GetDryObjectAt(c)?.ExitDryState(), // Swap Enter and Exit because it's moisture vs DryState
            (i, c) => i.GetDryObjectAt(c)?.EnterDryState()
        );
    }

    [HarmonyPostfix, HarmonyPatch(typeof(SoilContaminationService), nameof(SoilContaminationService.SoilIsContaminated))]
    public static void PatchSoilIsContaminated(Vector3Int coordinates, SoilContaminationService __instance, ref bool __result)
    {
        if (MSettings.NoConUp) { return; }

        PatchSoilIs(
            ref __result,
            coordinates,
            __instance._mapIndexService,
            __instance._threadSafeColumnTerrainMap,
            __instance.SoilIsContaminated);
    }

    [HarmonyPrefix, HarmonyPatch(typeof(SoilContaminationMap), nameof(SoilContaminationMap.SetContaminationLevel))]
    public static void PatchSetContaminationLevel(SoilContaminationMap __instance, Vector3Int coordinates, int index3D, float newLevel)
    {
        if (MSettings.NoConUp) { return; }

        PatchSetSoilLevel(__instance, coordinates, newLevel,
            i => i._contaminationLevels[index3D],
            i => i._terrainService,
            (i, c) => i.GetContaminatedObjectAt(c)?.EnterContaminatedState(),
            (i, c) => i.GetContaminatedObjectAt(c)?.ExitContaminatedState()
        );
    }

    static void PatchSoilIs(
        ref bool __result,
        Vector3Int coordinates,
        MapIndexService map,
        IThreadSafeColumnTerrainMap terrainMap,
        Func<Vector3Int, bool> isSoilAt
    )
    {
        if (__result || !MSettings.WithoutGround) { return; }

        var index2D = map.CellToIndex(coordinates.XY());
        var ceiling = TryGetCeilingAtOrBelowHeight(terrainMap, index2D, coordinates.z);

        if (ceiling == coordinates.z) { return; }

        var newCoords = coordinates with { z = ceiling };
        __result = isSoilAt(newCoords);
    }

    static int TryGetCeilingAtOrBelowHeight(IThreadSafeColumnTerrainMap imap, int index2D, int z)
    {
        var map = (ThreadSafeColumnTerrainMap)imap;

        byte b = map._columnCounts[index2D];
        for (int i = b - 1; i >= 0; i--)
        {
            var index3D = i * map._verticalStride + index2D;
            var ceiling = map._terrainColumns[index3D].Ceiling;
            if (ceiling <= z)
            {
                return ceiling;
            }
        }

        return 0;
    }

    static void PatchSetSoilLevel<T>(
        T instance,
        Vector3Int coordinates,
        float newLevel,
        Func<T, float> prevFunc,
        Func<T, ITerrainService> terrainFunc,
        Action<T, Vector3Int> enterAction,
        Action<T, Vector3Int> exitAction
    ) where T : class
    {
        if (!MSettings.WithoutGround) { return; }

        var prev = prevFunc(instance);
        var enter = newLevel > 0f && prev <= 0f;
        var exit = newLevel <= 0f && prev > 0f;

        if (!(enter || exit)) { return; }

        var terrain = terrainFunc(instance);
        var maxZ = terrain.Size.z + 1;

        for (int z = coordinates.z + 1; z < maxZ; z++)
        {
            var curr = coordinates with { z = z, };
            if (terrain.Underground(curr)) { break; }
            if (enter)
            {
                enterAction(instance, curr);
            }
            else if (exit)
            {
                exitAction(instance, curr);
            }
        }
    }

}
