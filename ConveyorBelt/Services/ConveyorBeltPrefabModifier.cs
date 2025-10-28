namespace ConveyorBelt.Services;

public class ConveyorBeltPrefabModifier : IPrefabModifier
{
    static readonly FrozenSet<string> LiftPrefabNames = ["ConveyorLift.Folktails", "ConveyorLift.IronTeeth",];
    static readonly FrozenSet<string> AllPrefabNames = [
        "ConveyorBelt.Folktails", "ConveyorBelt.IronTeeth",
        .. LiftPrefabNames,
    ];


    public GameObject? Modify(GameObject prefab, PrefabSpec prefabSpec, GameObject original)
    {
        var spec = prefab.AddComponent<ConveyorBeltSpec>();
        spec.isLift = LiftPrefabNames.Contains(prefabSpec.PrefabName);

        return prefab;
    }

    public bool ShouldModify(string prefabName, PrefabSpec prefabSpec) => AllPrefabNames.Contains(prefabName);
}
