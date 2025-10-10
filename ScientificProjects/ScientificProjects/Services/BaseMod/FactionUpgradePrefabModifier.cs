namespace ScientificProjects.Services.BaseMod;

public class FactionUpgradePrefabModifier : IPrefabModifier
{
    public GameObject? Modify(GameObject prefab, PrefabSpec prefabSpec, GameObject original)
    {
        prefab.AddComponent<SPFactionUpgradeDescriberSpec>();

        prefab.RemoveComponent<MechanicalNodeSpec>();
        prefab.RemoveComponent<MechanicalBuildingSpec>();
        return prefab;
    }

    public bool ShouldModify(string prefabName, PrefabSpec prefabSpec) 
        => ScientificProjectsUtils.HasFtUpgradeEffect(prefabName) 
           || ScientificProjectsUtils.HasItUpgradeEffect(prefabName);
}
