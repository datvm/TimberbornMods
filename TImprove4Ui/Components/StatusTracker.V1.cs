namespace TImprove4Ui.Components;

public class StatusTracker : BaseComponent, IAwakableComponent
{

    readonly HashSet<StatusInstance> statuses = [];
    public IReadOnlyCollection<StatusInstance> Statuses => statuses;

    public event EventHandler<StatusInstance>? StatusAdded;

    public string PrefabName { get; private set; } = "";

    public void Awake()
    {
        PrefabName = GetComponent<TemplateSpec>().TemplateName;
    }

    public void AddStatus(StatusInstance status)
    {
        statuses.Add(status);
        StatusAdded?.Invoke(this, status);
    }

}
