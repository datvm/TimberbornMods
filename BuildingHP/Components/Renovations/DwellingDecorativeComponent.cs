
namespace BuildingHP.Components.Renovations;

public class DwellingDecorativeComponent : TickableComponent, IActivableRenovationComponent, IActiveRenovationDescriber
{
    readonly InstantEffect effect = new("DecorativeCarvings", .1f, 1);

#nullable disable
    Enterable enterable;
    RenovationSpec spec;
#nullable enable

    public bool Active => enabled;
    public Action<BuildingRenovation>? ActiveHandler { get; set; }
    readonly HashSet<NeedManager> needManagers = [];

    public override void StartTickable()
    {
        enabled = false;
        base.StartTickable();

        enterable = GetComponentFast<Enterable>();

        spec = this.GetRenovationComponent().RenovationService.GetSpec(DwellingDecorativeProvider.RenoId);
        this.ActivateIfAvailable(DwellingDecorativeProvider.RenoId);
    }

    public override void Tick()
    {
        if (needManagers.Count == 0) { return; }

        foreach (var n in needManagers)
        {
            n.ApplyEffect(effect);
        }
    }

    public void Activate()
    {
        enterable.EntererAdded += Enterable_EntererAdded;
        enterable.EntererRemoved += Enterable_EntererRemoved;
        PopulateList();

        enabled = true;
    }

    private void Enterable_EntererRemoved(object sender, EntererRemovedEventArgs e)
    {
        var n = e.Enterer.GetComponentFast<NeedManager>();
        needManagers.Remove(n);
    }

    private void Enterable_EntererAdded(object sender, EntererAddedEventArgs e)
    {
        var n = e.Enterer.GetComponentFast<NeedManager>();
        if (n)
        {
            needManagers.Add(n);
        }
    }

    void PopulateList()
    {
        needManagers.Clear();

        foreach (var e in enterable.EnterersInside)
        {
            var n = e.GetComponentFast<NeedManager>();
            if (n)
            {
                needManagers.Add(n);
            }
        }
    }

    public ActiveRenovationDescription? Describe(ILoc t, IDayNightCycle dayNightCycle) 
        => Active ? new(spec.Title.Value, spec.Description) : null;
}
