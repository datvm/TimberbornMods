namespace HydroFormaProjects.Services;

public class HFSPPrefabModifier(ScientificProjectUnlockRegistry unlocks) : IPrefabModifier
{
    static readonly Directions3D AllDirections = (Directions3D)(1 + 2 + 4 + 8 + 0x10 + 0x20);

    public GameObject? Modify(GameObject prefab, PrefabSpec prefabSpec, GameObject original) => 
        prefabSpec.PrefabName switch
        {
            "Dam.Folktails" or "Dam.IronTeeth" => ModifyDam(prefab),
            "Levee.Folktails" or "Levee.IronTeeth" => ModifyLevee(prefab),
            "DirtExcavator.Folktails" or "DirtExcavator.IronTeeth" => ModifyDirtExcavator(prefab),
            "ContaminationBarrier.Folktails" or "IrrigationBarrier.IronTeeth" => ModifyBarriers(prefab),
            "ImpermeableFloor.Folktails" or "ImpermeableFloor.IronTeeth" => ModifyImpermeableFloor(prefab),
            _ => null,
        };

    GameObject ModifyDam(GameObject prefab)
    {
        prefab.RemoveComponent<FinishableWaterObstacleSpec>();
        prefab.AddComponent<DamGateComponentSpec>();

        return prefab;
    }

    GameObject ModifyLevee(GameObject prefab)
    {
        var mechNode = prefab.AddComponent<MechanicalNodeSpec>();
        mechNode._isShaft = true;

        var transput = prefab.AddComponent<TransputProviderSpec>();
        transput._transputSpecs = [
            new(Vector3Int.zero, AllDirections)
        ];

        prefab.AddComponent<ShaftSoundEmitterSpec>();

        return prefab;
    }

    GameObject ModifyDirtExcavator(GameObject prefab)
    {
        var workplace = prefab.GetComponent<WorkplaceSpec>();
        var modifier = 1f / workplace._maxWorkers;
        workplace._maxWorkers = workplace._defaultWorkers = 1;

        var multiplier = prefab.AddComponent<RecipeTimeMultiplierSpec>();
        multiplier.multiplier = modifier;
        multiplier.id = "HFDirtExcavatorUpgrade";

        return prefab;
    }

    GameObject ModifyBarriers(GameObject prefab)
    {
        const BlockOccupations RemovePath = ~BlockOccupations.Path;

        var blockSpec = prefab.GetComponent<BlockObjectSpec>();
        var blocks = blockSpec._blocksSpec._blockSpecs;
        for (int i = 0; i < blocks.Length; i++)
        {
            var value = blocks[i];
            blocks[i] = value with { _occupations = value._occupations & RemovePath };
        }

        return prefab;
    }

    GameObject ModifyImpermeableFloor(GameObject prefab)
    {
        var navMesh = prefab.GetComponent<BlockObjectNavMeshSettingsSpec>();
        navMesh._edgeGroups = [
            new()
            {
                _cost = .25f,
                _isPath = true,
                _addedEdges = [
                    new() { _end = new(0,-1,0) },
                    new() { _end = new(0,1,0) },
                    new() { _end = new(1,0,0) },
                    new() { _end = new(-1,0,0) },
                ],
                _group = new(),
            }
        ];

        var placable = prefab.GetComponent<PlaceableBlockObjectSpec>();
        placable._canBeAttachedToTerrainSide = true;

        var blockSpec = prefab.GetComponent<BlockObjectSpec>();
        var blocks = blockSpec._blocksSpec._blockSpecs;
        for (int i = 0; i < blocks.Length; i++)
        {
            var value = blocks[i];
            blocks[i] = value with
            {
                _matterBelow = MatterBelow.Any,
            };
        }

        return prefab;
    }

    public bool ShouldModify(string prefabName, PrefabSpec prefabSpec) => prefabName switch
    {
        "Dam.Folktails" or "Dam.IronTeeth" => true,
        "Levee.Folktails" or "Levee.IronTeeth" => unlocks.Contains(HydroFormaModUtils.LeveeUpgrade),
        "DirtExcavator.Folktails" or "DirtExcavator.IronTeeth" => unlocks.Contains(HydroFormaModUtils.DirtExcavatorUpgrade),
        "ContaminationBarrier.Folktails" or "IrrigationBarrier.IronTeeth" => unlocks.Contains(HydroFormaModUtils.BarrierUpgrade),
        "ImpermeableFloor.Folktails" or "ImpermeableFloor.IronTeeth" => unlocks.Contains(HydroFormaModUtils.ImpermeableFloorUpgrade),
        _ => false,
    };

}
