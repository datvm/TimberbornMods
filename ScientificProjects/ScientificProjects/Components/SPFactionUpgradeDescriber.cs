namespace ScientificProjects.Components;

public class SPFactionUpgradeDescriberSpec : BaseComponent
{
}

public class SPFactionUpgradeDescriber : BaseComponent, IEntityEffectDescriber
{
    bool isFt;

    public void Awake()
    {
        isFt = ScientificProjectsUtils.WoodWorkshopPrefabNames.Contains(GetComponentFast<PrefabSpec>().PrefabName);
    }

    public EntityEffectDescription? Describe(ILoc t, IDayNightCycle dayNightCycle) 
        => isFt
            ? new(t.T("LV.SP.FtPlankUpgrade"), t.T("LV.SP.FtPlankUpgradeEffNew"))
            : new(t.T("LV.SP.ItSmelterUpgrade"), t.T("LV.SP.ItSmelterUpgradeEffNew"));

}
