namespace ModdableWeathers.WeatherModifiers;

public abstract class ModdableWeatherModifierBase<TSettings>(
    ModdableWeatherModifierSpecService specs,
    ModdableWeatherModifierSettingsService settingsService
) : ILoadableSingleton, IModdableWeatherModifier<TSettings>
    where TSettings : ModdableWeatherModifierSettings
{

    FrozenSet<string> lockedWeathers = [];
    FrozenSet<string> incompatibleModifierIds = [];

    public abstract string Id { get; }
    public ModdableWeatherModifierSpec Spec { get; protected set; } = null!;
    public TSettings Settings { get; protected set; } = null!;

    public bool Active { get; protected set; }

    public event WeatherModifierChangedEventHandler? WeatherModifierChanged;
    protected void RaiseWeatherModifierChanged(bool active, bool onLoad) => WeatherModifierChanged?.Invoke(this, active, onLoad);

    public void Load()
    {
        Spec = specs.SpecsById[Id];
        lockedWeathers = [..Spec.CompatibleWeathers
            .Where(q => q.Lock)
            .Select(q => q.WeatherId)];
        incompatibleModifierIds = [..Spec.IncompatibleModifierIds];

        ReloadSettings();
    }

    public void ReloadSettings()
    {
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

        // Then, add any locked or missing entries with default values
        foreach (var (k, v) in defaultValues)
        {
            if (weathers.ContainsKey(k) && !v.Lock) { continue; }

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
        if (stageDecision.Modifiers.Any(m => incompatibleModifierIds.Contains(m.Id)))
        {
            return 0;
        }

        var weatherId = stageDecision.Weather?.Id;

        return (weatherId is null || !ShouldEnable(weatherId, stageDecision.Cycle))
            ? 0
            : Settings.Weathers[weatherId].Chance;
    }

    bool ShouldEnable(string id, int cycle)
    {
        if (lockedWeathers.Contains(id)) { return true; }

        if (!Settings.Weathers.TryGetValue(id, out var weatherSettings))
        {
            return false;
        }

        return weatherSettings.Enabled && weatherSettings.StartCycle <= cycle;
    }

    public virtual void Start(DetailedWeatherStageReference stage, WeatherHistoryService history, bool onLoad)
    {
        Active = true;
        RaiseWeatherModifierChanged(true, onLoad);
    }

    public virtual void End()
    {
        Active = false;
        RaiseWeatherModifierChanged(false, false);
    }

    public override string ToString() => $"{Id} ({Spec.Name.Value})";

}
