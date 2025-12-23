namespace ModdableWeathers.Common.PrioritizedModifiers;

public abstract class ModdableWeatherPriorityTickModifierServiceBase<T> : ModdableWeatherPriorityModifierServiceBase<T>, ITickableSingleton
    where T : IWeatherEntityTickModifierEntry
{
    protected readonly IDayNightCycle dayNightCycle;
    float changePerTick;

    public float CurrentModifier { get; protected set; }
    public float TargetModifier { get; protected set; }

    public bool Ticking { get; protected set; }
    public event Action<bool>? OnTickingChanged;
    protected void RaiseOnTickingChanged() => OnTickingChanged?.Invoke(Ticking);

    public ModdableWeatherPriorityTickModifierServiceBase(IDayNightCycle dayNightCycle)
    {
        this.dayNightCycle = dayNightCycle;

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
}