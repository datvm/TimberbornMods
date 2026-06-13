namespace ModdableTimberborn.EntityTracker;

public class TemplateTrackerComponent : BaseComponent, IAwakableComponent
{
    public TemplateSpec TemplateSpec { get; private set; } = null!;
    public string TemplateName { get; private set; } = null!;

    public void Awake()
    {
        TemplateSpec = GetComponent<TemplateSpec>();
        TemplateName = TemplateSpec.TemplateName;
    }
}
