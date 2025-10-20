namespace ModdableTimberborn.EntityTracker;

public interface IEntityTracker
{
    void Track(EntityComponent entity);
    void Untrack(EntityComponent entity);
}

public interface IEntityTracker<T> : IEntityTracker 
    where T : BaseComponent
{
    IReadOnlyCollection<T> Entities { get; }

    event Action<T>? OnEntityRegistered;
    event Action<T>? OnEntityUnregistered;
}
