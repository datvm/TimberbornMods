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
            _ => prefab,
        };
    }

    GameObject ModifyDam(GameObject prefab, PrefabSpec spec)
    {
        prefab.AddComponent<DamGateComponentSpec>();
        Object.Destroy(spec.GetComponentFast<FinishableWaterObstacleSpec>());

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

}
