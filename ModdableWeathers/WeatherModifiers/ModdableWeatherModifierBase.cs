namespace ModdableWeathers.WeatherModifiers;

public abstract class ModdableWeatherModifierBase<TSettings>(
    ModdableWeatherModifierSpecService specs,
    ModdableWeatherModifierSettingsService settingsService
) : ILoadableSingleton, IModdableWeatherModifier<TSettings>
    where TSettings : ModdableWeatherModifierSettings
{

    public abstract string Id { get; }
    public ModdableWeatherModifierSpec Spec { get; protected set; } = null!;
    public TSettings Settings { get; protected set; } = null!;

    public bool Enabled { get; protected set; }
    public bool Active { get; protected set; }

    public event WeatherModifierChangedEventHandler? WeatherModifierChanged;
    protected void RaiseWeatherModifierChanged(bool active, bool onLoad) => WeatherModifierChanged?.Invoke(this, active, onLoad);

    public void Load()
    {
        Spec = specs.SpecsById[Id];
        Settings = settingsService.GetSettings<TSettings>();
        InitializeSettings();
    }

    protected virtual void InitializeSettings()
    {
        var defaultValues = Spec.CompatibleWeathers
            .ToDictionary(q => q.WeatherId, q => q);
        var weathers = Settings.Weathers;

        // First, clear out any settings entries that are no longer valid
        foreach (var k in weathers.Keys.ToArray())
        {
            if (!defaultValues.ContainsKey(k))
            {
                weathers.Remove(k);
            }
        }

        // Then, add any missing entries with default values
        foreach (var (k, v) in defaultValues)
        {
            if (weathers.ContainsKey(k)) { continue; }

            weathers[k] = new()
            {
                Enabled = v.Lock || v.DefaultEnabled,
                Chance = v.Chance,
                StartCycle = v.StartCycle,
                Lock = v.Lock
            };
        }
    }

    public virtual int GetChance(WeatherCycleStageDecision stageDecision, WeatherCycleDecision cycleDecision, WeatherHistoryService history)
    {
        if (!Enabled) { return 0; } // Should not happen but just in case

        var weatherId = stageDecision.Weather?.Id;
        if (weatherId is null
            || !Settings.Weathers.TryGetValue(weatherId, out var weatherSettings)
            || weatherSettings.StartCycle < cycleDecision.Cycle)
        {
            return 0;
        }

        return weatherSettings.Chance;
    }

    public virtual void Start(DetailedWeatherCycle cycle, DetailedWeatherCycleStage stage, bool onLoad)
    {
        Active = true;
        RaiseWeatherModifierChanged(true, onLoad);
    }

    public virtual void End()
    {
        Active = false;
        RaiseWeatherModifierChanged(false, false);
    }

}
