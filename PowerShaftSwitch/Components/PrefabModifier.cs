namespace PowerShaftSwitch.Components;

public class PrefabModifier : IPrefabModifier
{
    static readonly ImmutableHashSet<string> Prefabs = ["PowerShaft.Folktails", "PowerShaft.IronTeeth"];

    public int Priority { get; }

    public GameObject ModifyPrefab(GameObject prefab)
    {
        var spec = prefab.GetComponent<PrefabSpec>();

        if (spec && Prefabs.Contains(spec.PrefabName))
        {
            prefab.AddComponent<TransputSwitchComponentSpec>();
        }

        return prefab;
    }
}
