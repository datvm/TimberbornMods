namespace ModdableWeathers.Common.PrioritizedModifiers;

public abstract class ModdableWeatherPriorityTickModifierServiceBase<T>
    : ModdableWeatherPriorityModifierServiceBase<T>
    , ITickableSingleton, ISaveableSingleton, ILoadableSingleton
    where T : IWeatherEntityTickModifierEntry
{
    static readonly PropertyKey<float> CurrentModifierKey = new("CurrentModifier");

    protected readonly IDayNightCycle dayNightCycle;
    protected readonly ISingletonLoader loader;
    float changePerTick;

    protected abstract SingletonKey SaveKey { get; }


    public float CurrentModifier { get; protected set; }
    public float TargetModifier { get; protected set; }

    public bool Ticking { get; protected set; }
    public event Action<bool>? OnTickingChanged;
    protected void RaiseOnTickingChanged() => OnTickingChanged?.Invoke(Ticking);

    public ModdableWeatherPriorityTickModifierServiceBase(IDayNightCycle dayNightCycle, ISingletonLoader loader)
    {
        this.dayNightCycle = dayNightCycle;
        this.loader = loader;

        CurrentModifier = Default.Target;
    }

    public void Tick()
    {
        if (!Ticking) { return; }

        if (CurrentModifier == TargetModifier)
        {
            if (Ticking)
            {
                Ticking = false;
                RaiseOnTickingChanged();
            }

            return;
        }

        var prev = TargetModifier < CurrentModifier;

        CurrentModifier += changePerTick;
        if (prev != (TargetModifier < CurrentModifier))
        {
            CurrentModifier = TargetModifier;
        }
    }

    protected override void ActivateNewModifier()
    {
        base.ActivateNewModifier();

        var highest = ActiveEntry;

        TargetModifier = highest.Target;
        changePerTick = (highest.Target - CurrentModifier)
            * dayNightCycle.FixedDeltaTimeInHours / highest.Hours;

        if (TargetModifier != CurrentModifier && !Ticking)
        {
            Ticking = true;
            RaiseOnTickingChanged();
        }
    }

    public virtual void Load()
    {
        if (loader.TryGetSingleton(SaveKey, out var s))
        {
            LoadSavedData(s);
        }
    }

    protected virtual void LoadSavedData(IObjectLoader s)
    {
        if (s.Has(CurrentModifierKey))
        {
            CurrentModifier = s.Get(CurrentModifierKey);
        }
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        var s = singletonSaver.GetSingleton(SaveKey);
        s.Set(CurrentModifierKey, CurrentModifier);
    }
}