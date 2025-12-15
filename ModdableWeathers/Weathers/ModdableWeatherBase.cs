namespace ModdableWeathers.Weathers;

public abstract class ModdableWeatherBase(ModdableWeatherSpecService specs) : IModdableWeather, ILoadableSingleton
{

    public abstract string Id { get; }
    public ModdableWeatherSpec Spec { get; protected set; } = null!;

    public bool Enabled { get; protected set; } = true;
    public bool Active { get; protected set; }

    public event WeatherChangedEventHandler? WeatherChanged;
    protected void RaiseWeatherChanged(bool active, bool onLoad)
    {
        WeatherChanged?.Invoke(this, active, onLoad);
    }

    public virtual void Load()
    {
        Spec = specs.SpecsById[Id];
    }

    public virtual void Start(bool onLoad)
    {
        Active = true;
        RaiseWeatherChanged(true, onLoad);
    }

    public void End()
    {
        Active = false;
        RaiseWeatherChanged(false, false);
    }

    public abstract int GetChance(int cycle, WeatherHistoryService history);
    public abstract int GetDurationAtCycle(int cycle, WeatherHistoryService history);
}
