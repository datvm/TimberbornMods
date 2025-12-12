namespace ModdableWeather.Weathers;

public abstract class ModdableWeatherBase
{
    public abstract string Id { get; }
    public ModdableWeatherSpec Spec { get; internal set; } = null!;

    public bool IsBenign { get; }
    public bool IsHazardous { get; }

    public bool Enabled { get; protected set; }
    public bool Active { get; protected set; }

    public event WeatherChangedEventHandler? OnActiveChanged;
    protected void RaiseOnActiveChanged(bool active, bool onLoad) => OnActiveChanged?.Invoke(this, active, onLoad);

    public ModdableWeatherBase()
    {
        IsBenign = this is IModdableBenignWeather;
        IsHazardous = this is IModdableHazardousWeather;

        if (!IsBenign && !IsHazardous)
        {
            throw new InvalidOperationException($"ModdableWeather '{Id}' ({GetType().FullName}) must implement either" +
                $" {nameof(IModdableBenignWeather)} or {nameof(IModdableHazardousWeather)}.");
        }
    }

    public virtual void Start(bool onLoad)
    {
        Active = true;
        RaiseOnActiveChanged(true, onLoad);
    }

    public void End()
    {
        Active = false;
        RaiseOnActiveChanged(false, false);
    }

    public abstract int GetChance(int cycle, ModdableWeatherHistoryProvider history);
    public abstract int GetDurationAtCycle(int cycle, ModdableWeatherHistoryProvider history);

    // Should not be called anymore, here for interface compatibility
    public int GetDurationAtCycle(int cycle) => throw new NotSupportedException();

}
