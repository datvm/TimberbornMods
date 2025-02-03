namespace TImprove.Services;
public class ModSettings : ModSettingsOwner
{
    public static ModSettings? Instance { get; private set; }

    public event Action OnSettingsChanged = delegate { };

    public static readonly string[] Lights = ["Sunrise", "Day", "Sunset", "Night"];

    protected override string ModId => nameof(TImprove);
    public override ModSettingsContext ChangeableOn => ModSettingsContext.All;

    ModSetting<bool>? enableFreeCamera, disableFog, prioritizeRubbles, allDayLight, showGameTime, enableSpeedS25, enableSpeed4, enableSpeed5, quickQuit, pauseBadWeather;
    LimitedStringModSetting? allDayLightValue;
    IEnumerable<ModSetting<bool>?> AllBoolSettings => [enableFreeCamera, disableFog, allDayLight, showGameTime, enableSpeedS25, enableSpeed4, enableSpeed5, quickQuit, pauseBadWeather, prioritizeRubbles];

    public bool EnableFreeCamera => enableFreeCamera?.Value == true;
    public bool DisableFog => disableFog?.Value == true;
    public bool PauseBadWeather => pauseBadWeather?.Value == true;
    public bool PrioritizeRubbles => prioritizeRubbles?.Value == true;

    public bool AllDayLight => allDayLight?.Value == true;
    public string StaticDayLight => allDayLightValue?.Value ?? Lights[1];

    public bool ShowGameTime => showGameTime?.Value == true;

    public bool EnableSpeedS25 => enableSpeedS25?.Value == true;
    public bool EnableSpeed4 => enableSpeed4?.Value == true;
    public bool EnableSpeed5 => enableSpeed5?.Value == true;

    public bool QuickQuit => quickQuit?.Value == true;

    public ModSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository) : base(settings, modSettingsOwnerRegistry, modRepository)
    {
        Instance = this;
    }

    protected override void OnAfterLoad()
    {
        enableFreeCamera = new(false, ModSettingDescriptor
            .CreateLocalized("LV.TI.EnableFreeCamera")
            .SetLocalizedTooltip("LV.TI.EnableFreeCameraDesc"));
        disableFog = new(false, ModSettingDescriptor
            .CreateLocalized("LV.TI.DisableFog")
            .SetLocalizedTooltip("LV.TI.DisableFogDesc"));
        pauseBadWeather = new(false, ModSettingDescriptor
            .CreateLocalized("LV.TI.PauseBadWeather")
            .SetLocalizedTooltip("LV.TI.PauseBadWeatherDesc"));
        prioritizeRubbles = new(false, ModSettingDescriptor
            .CreateLocalized("LV.TI.PrioritizeRubbles")
            .SetLocalizedTooltip("LV.TI.PrioritizeRubblesDesc"));

        allDayLight = new(false, ModSettingDescriptor
            .CreateLocalized("LV.TI.AllDayLight")
            .SetLocalizedTooltip("LV.TI.AllDayLightDesc"));
        allDayLightValue = new(1,
            Lights
                .Select(q => new LimitedStringModSettingValue(q, $"LV.TI.Light{q}"))
                .ToArray(),
            ModSettingDescriptor
                .CreateLocalized("LV.TI.StaticLight")
                .SetLocalizedTooltip("LV.TI.StaticLightDesc")
                .SetEnableCondition(() => allDayLight.Value));

        showGameTime = new(true, ModSettingDescriptor
            .CreateLocalized("LV.TI.ShowGameTime")
            .SetLocalizedTooltip("LV.TI.ShowGameTimeDesc"));

        enableSpeedS25 = new(false, ModSettingDescriptor
            .CreateLocalized("LV.TI.EnableSpeedS25")
            .SetLocalizedTooltip("LV.TI.EnableSpeedS25Desc"));
        enableSpeed4 = new(true, ModSettingDescriptor
            .CreateLocalized("LV.TI.EnableSpeed4")
            .SetLocalizedTooltip("LV.TI.EnableSpeed4Desc"));
        enableSpeed5 = new(false, ModSettingDescriptor
            .CreateLocalized("LV.TI.EnableSpeed5")
            .SetLocalizedTooltip("LV.TI.EnableSpeed5Desc"));

        quickQuit = new(false, ModSettingDescriptor
            .CreateLocalized("LV.TI.QuickQuit")
            .SetLocalizedTooltip("LV.TI.QuickQuitDesc"));

        AddCustomModSetting(enableFreeCamera, nameof(enableFreeCamera));
        AddCustomModSetting(disableFog, nameof(disableFog));
        AddCustomModSetting(pauseBadWeather, nameof(pauseBadWeather));
        AddCustomModSetting(prioritizeRubbles, nameof(prioritizeRubbles));

        AddCustomModSetting(allDayLight, nameof(allDayLight));
        AddCustomModSetting(allDayLightValue, nameof(allDayLightValue));

        AddCustomModSetting(showGameTime, nameof(showGameTime));

        AddCustomModSetting(enableSpeedS25, nameof(enableSpeedS25));
        AddCustomModSetting(enableSpeed4, nameof(enableSpeed4));
        AddCustomModSetting(enableSpeed5, nameof(enableSpeed5));

        AddCustomModSetting(quickQuit, nameof(quickQuit));

        foreach (var s in AllBoolSettings)
        {
            if (s is null) { continue; }

            s.ValueChanged += (_, _) => OnSettingsChanged();
        }
        allDayLightValue.ValueChanged += (_, _) => OnSettingsChanged();
    }

}
