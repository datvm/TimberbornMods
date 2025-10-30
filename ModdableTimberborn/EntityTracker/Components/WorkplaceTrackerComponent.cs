namespace ModdableTimberborn.EntityTracker;

public class WorkplaceTrackerComponent : BaseComponent, IAwakableComponent
{

#nullable disable
    public Workplace Workplace { get; private set; }
    public bool IsBuilderWorkplace { get; private set; }
    public string TemplateName { get; private set; }
#nullable enable

    public void Awake()
    {
        TemplateName = GetComponent<TemplateSpec>().TemplateName;

        Workplace = GetComponent<Workplace>();
        IsBuilderWorkplace = Workplace.IsBuilderWorkplace();
    }

}
