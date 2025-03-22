using System.Reflection.Emit;
using Timberborn.AreaSelectionSystem;
using Timberborn.GameDistrictsUI;
using Timberborn.MapIndexSystem;
using Timberborn.PlantingUI;
using Timberborn.SoilMoistureSystem;
using Timberborn.TerrainQueryingSystem;
using static Timberborn.AreaSelectionSystem.AreaPicker;

namespace VerticalFarming.Patches;

[HarmonyPatch]
public static class AreaPickerPatches
{

    [HarmonyPrefix, HarmonyPatch(typeof(AreaPicker), nameof(AreaPicker.PickTerrainIntArea))]
    public static bool PatchPickTerrainIntArea(IntAreaCallback previewCallback, IntAreaCallback actionCallback, Action showNoneCallback, AreaPicker __instance, ref bool __result)
    {
        if (!MSettings.WithoutGround) { return true; }

        __result = __instance._areaSelectionController.ProcessInput(
            (start, end, _) =>
            {
                previewCallback(ModifiedGetTerrainBlocks(start, end, __instance, false), start);
            },
            (start, end, _) =>
            {
                var blocks = ModifiedGetTerrainBlocks(start, end, __instance, true);
                actionCallback(blocks, start);
            },
            showNoneCallback
        );

        return false;
    }

    static IEnumerable<Vector3Int> ModifiedGetTerrainBlocks(Ray startRay, Ray endRay, AreaPicker instance, bool final)
    {
        var cursor = ModCursorService.Instance?.Cursor;
        if (cursor is null) { return []; }

        if (final)
        {
            Debug.Log("Start Ray: " + startRay);
            Debug.Log("End Ray: " + endRay);
        }

        if (!cursor.TryGetBlockObjectCoordinates(startRay, out var startCoord) ||
            !cursor.TryGetBlockObjectCoordinates(endRay, out var endCoord))
        {
            if (final)
            {
                Debug.Log("Has start: " + (startCoord is not null));
            }

            return [];
        }

        var start = startCoord!.Value.TileCoordinates;
        var end = endCoord!.Value.TileCoordinates with { z = start.z, };

        var blocks = instance._areaIterator.GetRectangle(start, end, MaxBlocksReturned);
        if (final)
        {
            Debug.Log("Start: " + start);
            Debug.Log("End: " + end);
            Debug.Log("Blocks: " + string.Join(", ", blocks));
        }

        return blocks;
    }

    [HarmonyTranspiler, HarmonyPatch(typeof(PlantingSelectionService), nameof(PlantingSelectionService.HighlightMarkableArea))]
    public static IEnumerable<CodeInstruction> TranspilerHighlightMarkableArea(IEnumerable<CodeInstruction> instructions)
        => RemoveInMapLeveledCoordinatesInstructions(instructions);

    [HarmonyTranspiler, HarmonyPatch(typeof(PlantingSelectionService), nameof(PlantingSelectionService.HighlightUnmarkableArea))]
    public static IEnumerable<CodeInstruction> TranspilerHighlightUnmarkableArea(IEnumerable<CodeInstruction> instructions)
        => RemoveInMapLeveledCoordinatesInstructions(instructions);

    [HarmonyTranspiler, HarmonyPatch(typeof(PlantingSelectionService), nameof(PlantingSelectionService.ActInArea))]
    public static IEnumerable<CodeInstruction> TranspilerActInArea(IEnumerable<CodeInstruction> instructions)
        => RemoveInMapLeveledCoordinatesInstructions(instructions);

    [HarmonyPostfix, HarmonyPatch(typeof(PlantingSoilValidator), nameof(PlantingSoilValidator.Validate))]
    public static void PatchPlantingSoilValidator(PlantingSpot plantingSpot, ref bool __result, PlantingSoilValidator __instance)
    {
        if (__result || !MSettings.WithoutGround) { return; }

    }

    [HarmonyPostfix, HarmonyPatch(typeof(SoilMoistureService), nameof(SoilMoistureService.SoilIsMoist))]
    public static void PatchSoilIsMoist(Vector3Int coordinates, SoilMoistureService __instance, ref bool __result)
    {
        if (__result || !MSettings.WithoutGround) { return; }

        var index2D = __instance._mapIndexService.CellToIndex(coordinates.XY());
        var ceiling = __instance._threadSafeColumnTerrainMap.GetCeilingAtOrBelowHeight(index2D, coordinates.z);
        if (ceiling == coordinates.z) { return; }

        var newCoords = coordinates with { z = ceiling };
        __result = __instance.SoilIsMoist(newCoords);
    }

    static IEnumerable<CodeInstruction> RemoveInMapLeveledCoordinatesInstructions(IEnumerable<CodeInstruction> instructions)
    {
        if (!MSettings.WithoutGround) { return instructions; }

        var list = instructions.ToList();

        // Find the InMapLeveledCoordinates call and remove it
        var index = list.FindIndex(ins =>
        {
            if (ins.opcode != OpCodes.Callvirt
                || ins.operand is not MethodInfo met) { return false; }

            return met.Name == nameof(TerrainAreaService.InMapLeveledCoordinates);
        });
        if (index < 0)
        {
            throw new InvalidOperationException("InMapLeveledCoordinates not found");
        }

        // This is the IL code: remove the call to InMapLeveledCoordinates and iterate inputBlocks (the first argument) directly instead
        // IL_0000: ldarg.0
        // IL_0001: ldfld class [Timberborn.TerrainQueryingSystem]Timberborn.TerrainQueryingSystem.TerrainAreaService Timberborn.PlantingUI.PlantingSelectionService::_terrainAreaService
        // IL_0006: ldarg.1
        // IL_0007: ldarg.2
        // IL_0008: callvirt instance class [netstandard]System.Collections.Generic.IEnumerable`1<valuetype [UnityEngine.CoreModule]UnityEngine.Vector3Int> [Timberborn.TerrainQueryingSystem]Timberborn.TerrainQueryingSystem.TerrainAreaService::InMapLeveledCoordinates(class [netstandard]System.Collections.Generic.IEnumerable`1<valuetype [UnityEngine.CoreModule]UnityEngine.Vector3Int>, valuetype [UnityEngine.CoreModule]UnityEngine.Ray)
        // IL_000d: callvirt instance class [netstandard]System.Collections.Generic.IEnumerator`1<!0> class [netstandard]System.Collections.Generic.IEnumerable`1<valuetype [UnityEngine.CoreModule]UnityEngine.Vector3Int>::GetEnumerator()
        // IL_0012: stloc.0

        index -= 4;
        for (int i = 0; i < 4; i++)
        {
            if (i == 2) { index++; }
            list.RemoveAt(index);
        }

        return list;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(PlantingSelectionService), nameof(PlantingSelectionService.ActInArea))]
    public static bool PatchActInArea(IEnumerable<Vector3Int> inputBlocks, Ray ray, Predicate<Vector3Int> predicate, Action<Vector3Int> action, PlantingSelectionService __instance)
    {
        if (!MSettings.WithoutGround) { return true; }

        var flag = false;
        foreach (var item in inputBlocks)
        {
            if (predicate(item))
            {
                action(item);
                flag = true;
            }
        }

        if (flag)
        {
            __instance._gameUISoundController.PlayFieldPlacedSound();
        }

        return false;
    }

}
