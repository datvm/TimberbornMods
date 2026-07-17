namespace ModdableTimberborn.Areas;

public abstract class AreaTrackerHandle<T, TRegistration>
    where TRegistration : AreaTrackerRegistration
{

    public ImmutableArray<BoundsInt> Areas { get; }
    public FrozenSet<int> Segments { get; }

    protected readonly DeferredHashSet<T> entities = [];
    public IReadOnlyCollection<T> Entities => entities;

    public int Count => entities.Count;
    public bool HasAny => entities.Count > 0;

    public event EventHandler<T> OnEntityEntered = null!;
    public event EventHandler<T> OnEntityExited = null!;

    public AreaTrackerHandle(TRegistration registration, AreaSegmentService areaSegmentService)
    {
        Areas = [.. registration.Areas];
        if (Areas.Length == 0)
        {
            throw new ArgumentException("At least one area must be specified.", nameof(registration));
        }

        Segments = areaSegmentService.GetSegments(Areas).ToFrozenSet();
    }

    public void Initialize(IEnumerable<T> entities)
    {
        var set = this.entities.Collection;

        foreach (var e in entities)
        {
            if (IsEntityInAreas(e))
            {
                set.Add(e);
            }
        }
    }
    protected abstract bool IsEntityInAreas(T entity);

    public void OnNewEntity(T entity)
    {
        if (IsEntityInAreas(entity))
        {
            InternalAdd(entity);
        }
    }

    public void OnEntityRemoved(T entity)
    {
        if (entities.Contains(entity))
        {
            InternalRemove(entity);
        }
    }

    public void OnEntityUpdated(T entity)
    {
        var curr = entities.Contains(entity);
        var shouldBe = IsEntityInAreas(entity);
        if (curr == shouldBe) { return; }

        if (curr)
        {
            InternalRemove(entity);
        }
        else
        {
            InternalAdd(entity);
        }
    }

    protected void InternalAdd(T entity)
    {
        if (entities.Contains(entity)) { return; }

        entities.Add(entity);
        OnEntityEntered?.Invoke(this, entity);
    }

    protected void InternalRemove(T entity)
    {
        if (!entities.Contains(entity)) { return; }

        entities.Remove(entity);
        OnEntityExited?.Invoke(this, entity);
    }

}
