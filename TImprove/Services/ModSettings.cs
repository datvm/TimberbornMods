namespace TImprove.Services;
public class ModSettings : ModSettingsOwner
{
    public static MSettings? Instance { get; private set; }

    static readonly ImmutableList<FieldInfo> AllBoolSettings = typeof(MSettings).GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
        .Where(q => q.FieldType == typeof(ModSetting<bool>))
        .ToImmutableList();

    public event Action OnSettingsChanged = delegate { };

    public static readonly string[] Lights = ["Sunrise", "Day", "Sunset", "Night"];
    static readonly HashSet<string> DefaultTrues = [nameof(showGameTime), nameof(enableSpeed4)];

    protected override string ModId => nameof(TImprove);
    public override ModSettingsContext ChangeableOn => ModSettingsContext.All;
    readonly IModSettingsContextProvider context;

#pragma warning disable IDE0044, CS0649 // Actually set by Reflection
    ModSetting<bool>? enableFreeCamera, disableFog, pauseBadWeather, prioritizeRubbles, autoClearDeadTrees,

        showCoords, onlyShowHeight,

        allDayLight,

        showGameTime,
        enableSpeedS25, enableSpeed4, enableSpeed5,
        quickQuit;
#pragma warning restore IDE0044, CS0649

    LimitedStringModSetting? allDayLightValue;
    RangeIntModSetting? biggerBuildDragArea;

    public bool EnableFreeCamera => enableFreeCamera?.Value == true;
    public bool DisableFog => disableFog?.Value == true;
    public bool PauseBadWeather => pauseBadWeather?.Value == true;
    public bool PrioritizeRubbles => prioritizeRubbles?.Value == true;
    public bool AutoClearDeadTrees => autoClearDeadTrees?.Value == true;

    public bool ShowCoords => showCoords?.Value == true;
    public bool OnlyShowHeight => onlyShowHeight?.Value == true;

    public bool AllDayLight => allDayLight?.Value == true;
    public string StaticDayLight => allDayLightValue?.Value ?? Lights[1];

    public bool ShowGameTime => showGameTime?.Value == true;

    public bool EnableSpeedS25 => enableSpeedS25?.Value == true;
    public bool EnableSpeed4 => enableSpeed4?.Value == true;
    public bool EnableSpeed5 => enableSpeed5?.Value == true;

    public bool QuickQuit => quickQuit?.Value == true;
    public static int BiggerBuildDragArea { get; private set; }

    public ModSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository, IModSettingsContextProvider context) : base(settings, modSettingsOwnerRegistry, modRepository)
    {
        Instance = this;
        this.context = context;
    }

    protected override void OnAfterLoad()
    {
        allDayLightValue = new(1,
            Lights
                .Select(q => new LimitedStringModSettingValue(q, $"LV.TI.Light{q}"))
                .ToArray(),
            ModSettingDescriptor
                .CreateLocalized("LV.TI.StaticLight")
                .SetLocalizedTooltip("LV.TI.StaticLightDesc")
                .SetEnableCondition(() => allDayLight!.Value));

        biggerBuildDragArea = new(0, 0, 20, ModSettingDescriptor
            .CreateLocalized("LV.TI.BiggerBuildDragArea")
            .SetLocalizedTooltip("LV.TI.BiggerBuildDragAreaDesc")
            .SetEnableCondition(() => context.Context == ModSettingsContext.MainMenu));

        foreach (var item in AllBoolSettings)
        {
            var locName= "LV.TI." + item.Name[0..1].ToUpper() + item.Name[1..];
            var locDescName = locName + "Desc";

            var f = new ModSetting<bool>(
                DefaultTrues.Contains(item.Name),
                ModSettingDescriptor
                    .CreateLocalized(locName)
                    .SetLocalizedTooltip(locDescName));

            item.SetValue(this, f);

            AddCustomModSetting(f, item.Name);

            if (item.Name == nameof(allDayLight))
            {
                AddCustomModSetting(allDayLightValue, nameof(allDayLightValue));
            }

            f.ValueChanged += (_, _) => InternalOnSettingsChanged();
        }
        AddCustomModSetting(biggerBuildDragArea, nameof(biggerBuildDragArea));

        onlyShowHeight!.Descriptor.SetEnableCondition(() => showCoords!.Value);

        allDayLightValue.ValueChanged += (_, _) => InternalOnSettingsChanged();
        biggerBuildDragArea.ValueChanged += (_, _) => InternalOnSettingsChanged();

        InternalOnSettingsChanged();
    }

    void InternalOnSettingsChanged()
    {
        OnSettingsChanged();

        BiggerBuildDragArea = biggerBuildDragArea!.Value;
    }

}
