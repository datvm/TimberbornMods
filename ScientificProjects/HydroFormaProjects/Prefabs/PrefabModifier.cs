namespace HydroFormaProjects.Prefabs;

public class PrefabModifier(
    ScientificProjectUnlockManager unlockManager
) : IPrefabModifier
{
    static readonly Directions3D AllDirections = (Directions3D)(1 + 2 + 4 + 8 + 0x10 + 0x20);
    static readonly Dictionary<GameObject, GameObject> originalObjects = [];

    public int Priority { get; }

    public GameObject ModifyPrefab(GameObject prefab)
    {
        var spec = prefab.GetComponent<PrefabSpec>();
        if (!spec) { return prefab; }

        if (originalObjects.TryGetValue(prefab, out var original) && original)
        {
            originalObjects.Remove(prefab);
            prefab = original;
        }

        if (!prefab)
        {
            throw new InvalidOperationException("Prefab is somehow null");
        }

        var modified = spec.Name switch
        {
            "Dam.Folktails" or "Dam.IronTeeth" => ModifyDam(prefab),
            "Levee.Folktails" or "Levee.IronTeeth" => ModifyLevee(prefab),
            "DirtExcavator.Folktails" or "DirtExcavator.IronTeeth" => ModifyDirtExcavator(prefab),
            "ContaminationBarrier.Folktails" or "IrrigationBarrier.IronTeeth" => ModifyBarriers(prefab),
            _ => prefab,
        };

        if (original && prefab != modified)
        {
            originalObjects.Add(modified, original);
        }

        return modified;
    }

    GameObject Copy(GameObject o) => Object.Instantiate(o);

    GameObject ModifyDam(GameObject prefab)
    {
        prefab = Copy(prefab);

        DestroyIfExist<FinishableWaterObstacleSpec>(prefab);
        prefab.AddComponent<DamGateComponentSpec>();        

        return prefab;
    }

    GameObject ModifyLevee(GameObject prefab)
    {
        if (!unlockManager.Contains(HydroFormaModUtils.LeveeUpgrade)) { return prefab; }
        prefab = Copy(prefab);

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
        prefab = Copy(prefab);

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
        prefab = Copy(prefab);

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
            ScientificProjectsUtils.Log(() => $"Removing component {typeof(T).Name} from prefab {prefab.name}.");
            Object.DestroyImmediate(comp);
        }
        else
        {
            Debug.LogWarning($"Component {typeof(T).Name} not found in prefab {prefab.name}. It was expected to be removed.");
        }
    }

}
