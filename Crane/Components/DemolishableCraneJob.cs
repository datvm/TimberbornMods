namespace Crane.Components;

[AddTemplateModule2(typeof(Demolishable))]
public class DemolishableCraneJob(
    DemolishCraneJobProvider provider,
    EntityService entityService
) : BaseComponent, IInitializableEntity, IDeletableEntity, ICraneJob
{
    Demolishable demolishable = null!;
    BlockObject bo = null!;
    BuilderPrioritizable? prioritizable;
    BoundsInt workableArea;
    bool listed;

    public Priority Priority => prioritizable?.Priority ?? Priority.Normal;

    public bool IsAvailable => demolishable && demolishable.IsMarked && bo.CanDelete();

    public event EventHandler? AvailabilityChanged;

    public event EventHandler<PriorityChangedEventArgs>? PriorityChanged;

    public void InitializeEntity()
    {
        demolishable = GetComponent<Demolishable>();
        bo = GetComponent<BlockObject>();
        prioritizable = GetComponent<BuilderPrioritizable>();
        workableArea = bo.GetConstructionBounds();

        if (prioritizable)
        {
            prioritizable!.PriorityChanged += OnPriorityChanged;
        }

        demolishable.Marked += OnMarked;
        demolishable.Unmarked += OnUnmarked;
        if (demolishable.IsMarked)
        {
            ListJob();
        }
    }

    public void DeleteEntity()
    {
        if (demolishable)
        {
            demolishable.Marked -= OnMarked;
            demolishable.Unmarked -= OnUnmarked;
        }

        if (prioritizable)
        {
            prioritizable!.PriorityChanged -= OnPriorityChanged;
        }

        Unlist();
    }

    public bool IsForCrane(CraneComponent crane)
    {
        if (!demolishable || !demolishable.IsMarked || GetComponent<CraneComponent>() == crane)
        {
            return false;
        }

        if (GetComponent<CraneSectionComponent>() is { } section && section)
        {
            return false;
        }

        return crane.Tower.WorkingBounds.Overlaps(workableArea);
    }

    public void ProgressJob(CraneComponent crane, float hours)
    {
        if (!demolishable || !demolishable.IsMarked)
        {
            return;
        }

        demolishable.ProgressDemolition(hours);
        if (demolishable.DemolishingProgress >= 1f && bo.CanDelete())
        {
            entityService.Delete(demolishable);
        }
    }

    public void SetPriority(Priority priority)
    {
        prioritizable?.SetPriority(priority);
    }

    void OnMarked(object sender, EventArgs e) => ListJob();

    void OnUnmarked(object sender, EventArgs e)
    {
        AvailabilityChanged?.Invoke(this, EventArgs.Empty);
        Unlist();
    }

    void OnPriorityChanged(object sender, PriorityChangedEventArgs e) => PriorityChanged?.Invoke(this, e);

    void ListJob()
    {
        if (listed)
        {
            return;
        }

        listed = true;
        provider.NotifyNew(this);
    }

    void Unlist()
    {
        if (!listed)
        {
            return;
        }

        listed = false;
        provider.NotifyRemoved(this);
    }
}
