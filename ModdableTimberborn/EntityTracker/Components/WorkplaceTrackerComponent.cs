namespace ModdableTimberborn.EntityTracker;

public class WorkplaceTrackerComponent : BaseComponent
{

#nullable disable
    public Workplace Workplace { get; private set; }
    public bool IsBuilderWorkplace { get; private set; }
    public string PrefabName { get; private set; }
#nullable enable

    public void Awake()
    {
        PrefabName = GetComponentFast<PrefabSpec>().PrefabName;

        Workplace = GetComponentFast<Workplace>();
        IsBuilderWorkplace = Workplace.IsBuilderWorkplace();
    }

}
