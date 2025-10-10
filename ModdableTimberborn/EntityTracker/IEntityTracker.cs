namespace ModdableTimberborn.EntityTracker;

public interface IEntityTracker
{
    void Track(EntityComponent entity);
    void Untrack(EntityComponent entity);
}

public interface IEntityTracker<T> : IEntityTracker 
    where T : BaseComponent
{
    public IReadOnlyCollection<T> Entities { get; }

    public event Action<T>? OnEntityRegistered;
    public event Action<T>? OnEntityUnregistered;
}
