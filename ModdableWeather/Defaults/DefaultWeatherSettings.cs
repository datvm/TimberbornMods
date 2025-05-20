namespace ModdableWeather.Defaults;

public abstract class DefaultWeatherSettings(
    ISettings settings,
    ModSettingsOwnerRegistry modSettingsOwnerRegistry,
    ModRepository modRepository,
    ILoc t
) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository)
{

    public abstract override string ModId { get; }
    public abstract string WeatherNameLocKey { get; }
    public virtual string WeatherNameDisplay { get; protected set; } = null!;

    public override string HeaderLocKey => WeatherNameLocKey;

#nullable disable
    public ModSetting<bool> EnableWeather { get; protected set; }
    public ModSetting<int> StartCycle { get; protected set; }
    public RangeIntModSetting Chance { get; protected set; }
    public ModSetting<int> MinDay { get; set; }
    public ModSetting<int> MaxDay { get; set; }
    public RangeIntModSetting HandicapPerc { get; set; }
    public ModSetting<int> HandicapCycles { get; set; }
#nullable enable

    public override void OnBeforeLoad()
    {
        WeatherNameDisplay = t.T(WeatherNameLocKey);
        InititalizeSettings();
    }

    public virtual WeatherParameters Parameters => new(
        EnableWeather.Value,
        StartCycle.Value,
        Chance.Value,
        MinDay.Value,
        MaxDay.Value,
        HandicapPerc.Value,
        HandicapCycles.Value
    );

    protected virtual void InititalizeSettings()
    {
        EnableWeather = new(true, CreateDescriptor("EnableWeather", true));
        StartCycle = new(0, CreateDescriptor("StartCycle"));
        Chance = new(50, 0, 100, CreateDescriptor("Chance"));
        MinDay = new(4, CreateDescriptor("MinDay"));
        MaxDay = new(10, CreateDescriptor("MaxDay"));
        HandicapPerc = new(15, 1, 100, CreateDescriptor("HandicapPerc"));
        HandicapCycles = new(4, CreateDescriptor("HandicapCycles"));
    }

    ModSettingDescriptor CreateDescriptor(string name, bool doNotDisable = false)
    {
        var s = ModSettingDescriptor
            .Create(t.T("LV.MW." + name, WeatherNameDisplay))
            .SetLocalizedTooltip(t.T($"LV.MW.{name}Desc", WeatherNameDisplay));

        if (!doNotDisable)
        {
            s.SetEnableCondition(() => EnableWeather.Value);
        }

        return s;
    }
}
