namespace ModdableTimberbornDemo.Features.DI;

public class DemoDamPrefabModifier1 : IPrefabModifier, ILoadableSingleton
{
    public int Order { get; }

    public void Load()
    {
        Debug.Log("DemoDamPrefabModifier.Load is called. This should be before PrefabGroupService.Load and even other IPrefabModifier.Load");
    }

    public GameObject? Modify(GameObject prefab, PrefabSpec prefabSpec, GameObject original)
    {
        var building = prefab.GetComponent<BuildingSpec>();

        var currCost = building._buildingCost[0].Amount;
        var newCost = currCost / 2;
        Debug.Log($"Changing cost of {prefabSpec.Name} from {currCost} to {newCost}");

        building._buildingCost[0] = building._buildingCost[0] with { _amount = newCost, };

        return prefab;
    }

    public bool ShouldModify(string prefabName, PrefabSpec prefabSpec) => prefabName is "Dam.Folktails" or "Dam.IronTeeth";
}

public class DemoDamPrefabModifier2 : IPrefabModifier
{
    public int Order { get; } = 1; // Run after DemoDamPrefabModifier1

    public GameObject? Modify(GameObject prefab, PrefabSpec prefabSpec, GameObject original)
    {
        var building = prefab.GetComponent<BuildingSpec>();
        var currCost = building._buildingCost[0].Amount;
        var newCost = currCost + 1;

        Debug.Log($"Changing cost of {prefabSpec.Name} from {currCost} to {newCost}" +
            $" (original cost is {original.GetComponent<BuildingSpec>()._buildingCost[0].Amount})");
        building._buildingCost[0] = building._buildingCost[0] with { _amount = newCost, };

        return prefab;
    }
    public bool ShouldModify(string prefabName, PrefabSpec prefabSpec) => prefabName is "Dam.Folktails" or "Dam.IronTeeth";
}
