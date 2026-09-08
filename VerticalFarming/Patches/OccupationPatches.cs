namespace VerticalFarming.Patches;

[HarmonyPatch]
public static class OccupationPatches
{

    [HarmonyPostfix, HarmonyPatch(typeof(PrefabGroupService), nameof(PrefabGroupService.Load))]
    public static void PatchPlantDefinitions(PrefabGroupService __instance)
    {
        BlockOccupations removing = 0;
        if (MSettings.RemoveCorner) { removing |= BlockOccupations.Corners; }
        if (MSettings.RemovePath) { removing |= BlockOccupations.Path; }

        var removingOcc = removing != BlockOccupations.None;
        var withoutGround = MSettings.WithoutGround;

        if (!(removingOcc || withoutGround)) { return; }
        ModStarter.Log(() => $"Removing {removing} occupation");
        removing = ~removing;

        var plants = __instance.AllPrefabs.SelectMany(q => q.GetComponents<PlantableSpec>());
        foreach (var plant in plants)
        {
            var blockObjSpec = plant.GetComponentFast<BlockObjectSpec>();
            if (blockObjSpec is null) { continue; }

            ModStarter.Log(() => $"Modifying {plant.name}");

            var blocks = blockObjSpec._blocksSpec._blockSpecs;
            for (int i = 0; i < blocks.Length; i++)
            {
                if (removingOcc)
                {
                    blocks[i]._occupations &= removing;
                }

                if (withoutGround)
                {
                    if (blocks[i]._matterBelow == MatterBelow.Ground)
                    {
                        blocks[i]._matterBelow = MatterBelow.GroundOrStackable;
                    }
                }
            }

        }
    }

}
