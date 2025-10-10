namespace ScientificProjects.Components;

public class WorkplaceProjectUpgradeComponent : BaseComponent, IInitializableEntity, IDeletableEntity
{

#nullable disable
    public Workplace Workplace { get; private set; }
    public bool IsBuilderWorkplace { get; private set; }
    WorkplaceTracker workplaceTracker;
#nullable enable

    [Inject]
    public void Inject(WorkplaceTracker workplaceTracker)
    {
        this.workplaceTracker = workplaceTracker;
    }

    public void Awake()
    {
        Workplace = GetComponentFast<Workplace>();
        IsBuilderWorkplace = Workplace.IsBuilderWorkplace();
    }

    public void DeleteEntity()
    {
        workplaceTracker.Unregister(this);
    }

    public void InitializeEntity()
    {
        workplaceTracker.Register(this);
    }
}
