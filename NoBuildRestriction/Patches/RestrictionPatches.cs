global using Timberborn.BlockSystem;
global using Timberborn.Buildings;
global using Timberborn.PrefabGroupSystem;
global using Timberborn.WaterBuildings;

namespace NoBuildRestriction.Patches;

[HarmonyPatch]
public static class RestrictionPatches
{

    [HarmonyPostfix, HarmonyPatch(typeof(PrefabGroupService), nameof(PrefabGroupService.Load))]
    public static void RemoveRestriction(PrefabGroupService __instance)
    {
        if (!(MSettings.RemoveGroundOnly
            || MSettings.RemoveRoofOnly
            || MSettings.AllowFlooded
            || MSettings.AlwaysSolid
            || MSettings.SuperStructure
            || MSettings.PlatformOver1x1)) { return; }

        foreach (var prefab in __instance.AllPrefabs)
        {
            var building = prefab.GetComponent<BuildingSpec>();
            if (!building) { continue; }

            AddFloodBlocker(building, prefab);

            var blockObj = building.GetComponentFast<BlockObjectSpec>();
            var blocks = blockObj?._blocksSpec._blockSpecs;

            if (blocks is not null)
            {
                RemovePlacementRestriction(blockObj!, blocks);

                blocks = blockObj!._blocksSpec._blockSpecs; // Blocks may have changed
                Remove1x1Corners(blockObj, blocks);
                AddSolidTop(blockObj, blocks);
                AddSuperFoundation(blockObj, blocks, building);
            }
        }
    }

    static void RemovePlacementRestriction(BlockObjectSpec blockObj, BlockSpec[] blocks)
    {
        var removedGround = false;
        for (int i = 0; i < blocks.Length; i++)
        {
            if (MSettings.RemoveGroundOnly
                && ReplaceRestriction(MatterBelow.Ground, MatterBelow.GroundOrStackable, ref blocks[i]._matterBelow))
            {
                removedGround = true;
            }
            else if (MSettings.RemoveRoofOnly)
            {
                ReplaceRestriction(MatterBelow.Stackable, MatterBelow.GroundOrStackable, ref blocks[i]._matterBelow);
            }
        }

        var baseZ = blockObj._baseZ;
        if (removedGround && baseZ > 0)
        {
            var size = blockObj._blocksSpec._size;
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    for (int z = 0; z < size.z; z++)
                    {
                        var index = GetIndex(x, y, z, in size);
                        blocks[index]._matterBelow = z == baseZ ? MatterBelow.GroundOrStackable : MatterBelow.Any;
                        blocks[index]._underground = false;
                    }
                }
            }
        }
    }

    static bool ReplaceRestriction(MatterBelow from, MatterBelow to, ref MatterBelow value)
    {
        if (value == from)
        {
            value = to;
            return true;
        }
        return false;
    }

    static readonly ImmutableHashSet<string> Excluded1x1Buildings = ["TerrainBlock.Folktails", "TerrainBlock.IronTeeth", "Dynamite.Folktails", "Dynamite.IronTeeth"];
    static void Remove1x1Corners(BlockObjectSpec blockObj, BlockSpec[] blocks)
    {
        if (!MSettings.PlatformOver1x1 || Excluded1x1Buildings.Contains(blockObj.name)) { return; }

        var size = blockObj.BlocksSpec.Size;
        if (size.x != 1 || size.y != 1) { return; }

        Debug.Log($"Removing 1x1 corners from {blockObj.name}");

        var removingCorner = ~BlockOccupations.Corners;
        for (int z = size.z - 1; z >= 0; z--)
        {
            var index = GetIndex(0, 0, z, in size);

            var value = blocks[index]._occupations & removingCorner;
            if (value != BlockOccupations.None)
            {
                blocks[index]._occupations &= removingCorner;
            }
        }
    }

    static void AddSolidTop(BlockObjectSpec blockObj, BlockSpec[] blocks)
    {
        if (!MSettings.AlwaysSolid) { return; }

        var size = blockObj.BlocksSpec.Size;
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                for (int z = size.z - 1; z >= 0; z--)
                {
                    var index = GetIndex(x, y, z, in size);

                    var occupation = blocks[index].Occupation;

                    // Don't mark Path as Solid
                    if ((occupation | BlockOccupations.Path) != BlockOccupations.Path)
                    {
                        blocks[index]._stackable = BlockStackable.BlockObject;
                        break;
                    }
                }
            }
        }
    }

    static void AddSuperFoundation(BlockObjectSpec blockObj, BlockSpec[] blocks, BuildingSpec spec)
    {
        if (!MSettings.SuperStructure) { return; }

        var size = blockObj.BlocksSpec.Size;
        var mainX = size.x / 2;
        var mainY = size.y / 2;

        var hasEntrance = blockObj.Entrance.HasEntrance;
        var placable = spec.GetComponentFast<PlaceableBlockObjectSpec>();

        if (hasEntrance && placable)
        {
            var entrance = blockObj.Entrance.Coordinates;
            mainX = entrance.x;
            mainY = entrance.y + 1;

            placable._customPivot._hasCustomPivot = true;
            placable._customPivot._coordinates = new Vector3(mainX + .5f, mainY + .5f, 0);
        }

        if (MSettings.MagicStructure)
        {
            mainX = mainY = -1;

            if (MSettings.HangingStructure && placable)
            {
                placable._canBeAttachedToTerrainSide = true;
            }
        }

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                if (x == mainX && y == mainY) { continue; }

                for (int z = 0; z < size.z; z++)
                {
                    var index = GetIndex(x, y, z, in size);
                    if (blocks[index].MatterBelow != MatterBelow.Any)
                    {
                        blocks[index]._matterBelow = MatterBelow.Any;
                    }
                }
            }
        }


    }

    static void AddFloodBlocker(BuildingSpec building, GameObject prefab)
    {
        if (MSettings.AllowFlooded && building.GetComponentFast<FloodableBuildingBlockerSpec>() is null)
        {
            prefab.AddComponent<FloodableBuildingBlockerSpec>();
        }
    }

    [HarmonyPrefix, HarmonyPatch(typeof(FloodableBuilding), nameof(FloodableBuilding.Flood))]
    public static bool PatchFlooding()
    {
        if (!MSettings.AllowFlooded) { return true; }

        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(TerrainPhysicsPostLoader), nameof(TerrainPhysicsPostLoader.RemoveBlockObjects))]
    public static bool DontRemoveBlockObjects()
    {
        return !MSettings.SuperHangingTerrain && (!MSettings.SuperStructure || !MSettings.MagicStructure);
    }

    [HarmonyPrefix, HarmonyPatch(typeof(TerrainPhysicsPostLoader), nameof(TerrainPhysicsPostLoader.RemoveTerrain))]
    public static bool DontRemoveTerrain() => DontRemoveBlockObjects();

    static int GetIndex(int x, int y, int z, in Vector3Int size) => (z * size.y + y) * size.x + x;

}
