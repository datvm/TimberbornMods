namespace HydroFormaProjects.Prefabs;

public class PrefabModifier : IPrefabModifier
{
    public int Priority { get; }

    public GameObject ModifyPrefab(GameObject prefab)
    {
        var spec = prefab.GetComponent<PrefabSpec>();
        if (!spec) { return prefab; }

        return spec.Name switch
        {
            "Dam.Folktails" or "Dam.IronTeeth" => ModifyDam(prefab, spec),
            _ => prefab,
        };
    }

    GameObject ModifyDam(GameObject prefab, PrefabSpec spec)
    {
        prefab.AddComponent<DamGateComponent>();
        Object.Destroy(spec.GetComponentFast<FinishableWaterObstacleSpec>());

        return prefab;
    }

}
