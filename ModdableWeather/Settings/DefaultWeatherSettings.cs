namespace ModdableWeather.Settings;

public abstract class DefaultWeatherSettings(
    ISettings settings,
    ModSettingsOwnerRegistry modSettingsOwnerRegistry,
    ModRepository modRepository,
    ILoc t,
    ModdableWeatherSpecService specs
) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository), IExportableSettings
{

    public abstract string WeatherId { get; }

    string modId = null!;
    public override string ModId => modId;

    protected string WeatherNameLocKey { get; set; } = null!;
    public virtual string WeatherNameDisplay { get; protected set; } = null!;
    public override string HeaderLocKey => WeatherNameLocKey;

    public abstract WeatherParameters DefaultSettings { get; }

#nullable disable
    public ModSetting<bool> EnableWeather { get; protected set; }
    public ModSetting<int> StartCycle { get; protected set; }
    public ModSetting<int> Chance { get; protected set; }
    public ModSetting<int> MinDay { get; set; }
    public ModSetting<int> MaxDay { get; set; }
    public ModSetting<int> HandicapPerc { get; set; }
    public ModSetting<int> HandicapCycles { get; set; }
#nullable enable

    public override void OnBeforeLoad()
    {
        modId = GetType().Assembly.GetName().Name;

        var spec = specs.Specs[WeatherId];
        WeatherNameLocKey = spec.DisplayLoc;

        WeatherNameDisplay = t.T(WeatherNameLocKey);
        InititalizeSettings();
    }

    public override void OnAfterLoad()
    {
        Parameters = new(
            EnableWeather.Value,
            StartCycle.Value,
            Chance.Value,
            MinDay.Value,
            MaxDay.Value,
            HandicapPerc.Value,
            HandicapCycles.Value
        );
    }

    public WeatherParameters Parameters { get; protected set; } = null!;

    protected virtual void InititalizeSettings()
    {
        var inits = DefaultSettings;

        EnableWeather = new(inits.Enabled, CreateDescriptor("EnableWeather", true));
        StartCycle = new(inits.StartCycle, CreateDescriptor("StartCycle"));
        Chance = new(inits.Chance, CreateDescriptor("Chance"));
        MinDay = new(inits.MinDay, CreateDescriptor("MinDay"));
        MaxDay = new(inits.MaxDay, CreateDescriptor("MaxDay"));
        HandicapPerc = new(inits.HandicapPerc, CreateDescriptor("HandicapPerc"));
        HandicapCycles = new(inits.HandicapCycles, CreateDescriptor("HandicapCycles"));
    }

    ModSettingDescriptor CreateDescriptor(string name, bool doNotDisable = false)
    {
        var s = ModSettingDescriptor
            .Create(t.T("LV.MW." + name, WeatherNameDisplay))
            .SetTooltip(t.T($"LV.MW.{name}Desc", WeatherNameDisplay));

        if (!doNotDisable)
        {
            s.SetEnableCondition(() => EnableWeather.Value);
        }

        return s;
    }

    public virtual string Export() => JsonConvert.SerializeObject(GetExportObject());
    protected virtual object GetExportObject() => Parameters;

    public virtual void Import(string value)
    {
        var parameters = JsonConvert.DeserializeObject<WeatherParameters>(value);
        if (parameters is null) { return; }

        ImportFromParameters(parameters);
    }

    protected virtual void ImportFromParameters(WeatherParameters parameters)
    {
        EnableWeather.Value = parameters.Enabled;
        StartCycle.Value = parameters.StartCycle;
        Chance.Value = parameters.Chance;
        MinDay.Value = parameters.MinDay;
        MaxDay.Value = parameters.MaxDay;
        HandicapPerc.Value = parameters.HandicapPerc;
        HandicapCycles.Value = parameters.HandicapCycles;
    }

}
