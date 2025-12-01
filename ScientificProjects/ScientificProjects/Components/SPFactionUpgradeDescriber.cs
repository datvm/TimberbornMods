namespace ScientificProjects.Components;

public record SPFactionUpgradeDescriberSpec : ComponentSpec;

public class SPFactionUpgradeDescriber : BaseComponent, IEntityEffectDescriber, IAwakableComponent
{
    bool isFt;

    public void Awake()
    {
        isFt = ScientificProjectsUtils.WoodWorkshopTemplateNames.Contains(GetComponent<TemplateSpec>().TemplateName);
    }

    public EntityEffectDescription? Describe(ILoc t, IDayNightCycle dayNightCycle) 
        => isFt
            ? new(t.T("LV.SP.FtPlankUpgrade"), t.T("LV.SP.FtPlankUpgradeEffNew"))
            : new(t.T("LV.SP.ItSmelterUpgrade"), t.T("LV.SP.ItSmelterUpgradeEffNew"));

}
