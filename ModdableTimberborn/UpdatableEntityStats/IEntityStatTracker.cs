namespace ModdableTimberborn.UpdatableEntityStats;

public interface IStatTracker : IDisposable
{
    void Start();
    void Pause();
    bool Running { get; }

    string ValueFormatted { get; }
    event EventHandler? OnValueChanged;
}

public interface IStatTracker<T> : IStatTracker
{
    T? Value { get; }

    event EventHandler<T?>? OnTypedValueChanged;
}

public interface IPercentStatTracker : IStatTracker<float>
{
    string IStatTracker.ValueFormatted => Value.ToString("P0");
}

public interface IEntityPercentStatTracker : IPercentStatTracker, IEntityStatTracker<float> { }

public interface IEntityStatTracker : IStatTracker
{
    UpdatableEntityStatComponent Component { get; }

    void NotifyEntityLost();
    event EventHandler<UpdatableEntityStatComponent>? OnEntityLost;

}

public interface IEntityStatTracker<T> : IEntityStatTracker, IStatTracker<T>;
public interface IImageStatTracker : IStatTracker<Sprite> { }