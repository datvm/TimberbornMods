namespace HydroFormaProjects.Prefabs;

public class PrefabModifier(
    ScientificProjectUnlockManager unlockManager
) : IPrefabModifier
{
    static readonly Directions3D AllDirections = (Directions3D)(1 + 2 + 4 + 8 + 0x10 + 0x20);

    public int Priority { get; }

    public GameObject ModifyPrefab(GameObject prefab)
    {
        var spec = prefab.GetComponent<PrefabSpec>();
        if (!spec) { return prefab; }

        return spec.Name switch
        {
            "Dam.Folktails" or "Dam.IronTeeth" => ModifyDam(prefab, spec),
            "Levee.Folktails" or "Levee.IronTeeth" => ModifyLevee(prefab),
            "DirtExcavator.Folktails" or "DirtExcavator.IronTeeth" => ModifyDirtExcavator(prefab),
            "ContaminationBarrier.Folktails" or "IrrigationBarrier.IronTeeth" => ModifyBarriers(prefab),
            _ => prefab,
        };
    }

    GameObject ModifyDam(GameObject prefab, PrefabSpec spec)
    {
        prefab.AddComponent<DamGateComponentSpec>();
        DestroyIfExist<FinishableWaterObstacleSpec>(prefab);

        return prefab;
    }

    GameObject ModifyLevee(GameObject prefab)
    {
        if (!unlockManager.Contains(HydroFormaModUtils.LeveeUpgrade)) { return prefab; }

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
        if (!unlockManager.Contains(HydroFormaModUtils.DirtExcavatorUpgrade)) { return prefab; }

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
        if (!unlockManager.Contains(HydroFormaModUtils.BarrierUpgrade)) { return prefab; }

        var blockSpec = prefab.GetComponent<BlockObjectSpec>();
        var blocks = blockSpec._blocksSpec._blockSpecs;
        for (int i = 0; i < blocks.Length; i++)
        {
            var value = blocks[i];
            blocks[i] = value with { _occupations = value._occupations & RemovePath };
        }

        return prefab;
    }

    static void DestroyIfExist<T>(GameObject prefab) where T : Component
    {
        var comp = prefab.GetComponent<T>();
        if (comp)
        {
            Object.Destroy(comp);
        }
    }

}
