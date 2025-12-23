namespace ModdableWeathers.Weathers;

public abstract class ModdableWeatherBase(ModdableWeatherSpecService specs) : IModdableWeather, ILoadableSingleton
{

    public abstract string Id { get; }
    public ModdableWeatherSpec Spec { get; protected set; } = null!;

    public virtual bool Enabled { get; } = true;
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

    public virtual void Start(DetailedWeatherStageReference stage, bool onLoad)
    {
        Active = true;
        RaiseWeatherChanged(true, onLoad);
    }

    public void End()
    {
        Active = false;
        RaiseWeatherChanged(false, false);
    }

    public abstract int GetChance(WeatherCycleStageDecision stageDecision, WeatherCycleDecision cycleDecision, WeatherHistoryService history);
    public abstract int GetDuration(WeatherCycleStageDecision stageDecision, WeatherCycleDecision cycleDecision, WeatherHistoryService history);

    public override string ToString() => $"{Id} ({Spec.Display.Value})";

}
