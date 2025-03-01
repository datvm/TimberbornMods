﻿using Timberborn.BlockSystem;

namespace WaterSourcesDontCrash;

[HarmonyPatch]
public static class BlockObjectPatch
{

    [HarmonyPostfix, HarmonyPatch(typeof(BlockSpec), "MatterBelow", MethodType.Getter)]
    public static void GroundOnlyPrefix(ref MatterBelow __result)
    {
        if (__result == MatterBelow.Ground)
        {
            __result = MatterBelow.GroundOrStackable;
        }
    }

    [HarmonyPrefix, HarmonyPatch(typeof(Block), nameof(Block.Underground), MethodType.Getter)]
    public static bool UndergroundPrefix(ref bool __result)
    {
        __result = false;
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(BlockValidator), nameof(BlockValidator.BlocksValid), argumentTypes: [typeof(PositionedBlocks)])]
    public static bool BlocksValid(PositionedBlocks positionedBlocks, BlockValidator __instance, ref bool __result)
    {
        var blocks = positionedBlocks.GetAllBlocks();
        if (blocks.Length == 0) { return true; }

        var topZ = blocks.Max(block => block.Coordinates.z);
        var topBlocks = blocks.Where(block => block.Coordinates.z == topZ).ToImmutableArray();

        __result = topBlocks.All((Block block) => __instance.BlockValid(block, almost: false, ignoreUnfinishedStackable: false));
        return false;
    }

}