namespace ModdableTimberborn.EntityTracker;

public sealed class DefaultEntityTracker<T> : IEntityTracker<T>
    where T : BaseComponent
{
    readonly HashSet<T> entities = [];
    public IReadOnlyCollection<T> Entities => entities;

    public event Action<T>? OnEntityRegistered;
    public event Action<T>? OnEntityUnregistered;

    public void Track(EntityComponent entity)
    {
        var comp = entity.GetComponentFast<T>();
        if (!comp) { return; }

        entities.Add(comp);
        OnEntityRegistered?.Invoke(comp);
    }

    public void Untrack(EntityComponent entity)
    {
        var comp = entity.GetComponentFast<T>();
        if (!comp) { return; }

        entities.Remove(comp);
        OnEntityUnregistered?.Invoke(comp);
    }
}
