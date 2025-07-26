namespace TImprove.Settings;
public class MSettings(
    ISettings settings,
    ModSettingsOwnerRegistry modSettingsOwnerRegistry,
    ModRepository modRepository,
    IModSettingsContextProvider context
) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository),
    IUnloadableSingleton
{
    public static MSettings? Instance { get; private set; }

    static readonly ImmutableList<FieldInfo> AllBoolSettings = [..
        typeof(MSettings).GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
            .Where(q => q.FieldType == typeof(ModSetting<bool>))];

    public static readonly ImmutableArray<string> Lights = ["Sunrise", "Day", "Sunset", "Night"];
    static readonly ImmutableHashSet<string> DefaultTrues = [nameof(showGameTime), nameof(enableSpeed4)];
    static readonly ImmutableArray<ClearDeadStumpModeValue> ClearDeadStumpModes = TImproveUtils.GetEnumValues<ClearDeadStumpModeValue>();

    public override string ModId { get; } = nameof(TImprove);
    public override ModSettingsContext ChangeableOn => ModSettingsContext.All;

#pragma warning disable IDE0044, CS0649 // Actually set by Reflection
    ModSetting<bool>? enableFreeCamera, freeCameraLockAngle, disableFog, prioritizeRubbles,

        showCoords, onlyShowHeight,

        allDayLight, disableShadowRotation,

        showGameTime, addRealTimeClock, clock24,
        enableSpeedS25, enableSpeed4, enableSpeed5,
        quickQuit
    ;
#pragma warning restore IDE0044, CS0649

    LimitedStringModSetting? allDayLightValue;
    RangeIntModSetting? biggerBuildDragArea;

    public bool EnableFreeCamera => enableFreeCamera?.Value == true;
    public bool FreeCameraLockAngle => freeCameraLockAngle?.Value == true;
    public bool DisableFog => disableFog?.Value == true;
    public bool PrioritizeRubbles => prioritizeRubbles?.Value == true;

    public bool ShowCoords => showCoords?.Value == true;
    public bool OnlyShowHeight => onlyShowHeight?.Value == true;

    public bool AllDayLight => allDayLight?.Value == true;
    public string StaticDayLight => allDayLightValue?.Value ?? Lights[1];
    public bool DisableShadowRotation => disableShadowRotation?.Value == true;

    public bool ShowGameTime => showGameTime?.Value == true;
    public bool AddRealTimeClock => addRealTimeClock?.Value == true;
    public bool Clock24 => clock24?.Value == true;

    public bool EnableSpeedS25 => enableSpeedS25?.Value == true;
    public bool EnableSpeed4 => enableSpeed4?.Value == true;
    public bool EnableSpeed5 => enableSpeed5?.Value == true;

    public bool QuickQuit => quickQuit?.Value == true;
    public static int BiggerBuildDragArea { get; private set; }

    public ClearDeadStumpModeValue ClearDeadStumpValue { get; private set; }
    public LimitedStringModSetting ClearDeadStumpMode { get; } = TImproveUtils.CreateLimitedStringModSetting(ClearDeadStumpModes, "LV.TI.AutoClearDeadTrees");

    public override void OnBeforeLoad()
    {
        Instance = this;
    }

    public override void OnAfterLoad()
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
            var locName = "LV.TI." + item.Name[0..1].ToUpper() + item.Name[1..];
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
        }
        AddCustomModSetting(biggerBuildDragArea, nameof(biggerBuildDragArea));

        onlyShowHeight!.Descriptor.SetEnableCondition(() => showCoords!.Value);
        clock24!.Descriptor.SetEnableCondition(() => addRealTimeClock!.Value);

        ModSettingChanged += (_, _) => InternalOnSettingsChanged();
        InternalOnSettingsChanged();
    }

    void InternalOnSettingsChanged()
    {
        
        BiggerBuildDragArea = biggerBuildDragArea!.Value;
        ClearDeadStumpValue = Enum.Parse<ClearDeadStumpModeValue>(ClearDeadStumpMode.Value);
    }

    public void Unload()
    {
        Instance = null;
    }

}

public enum ClearDeadStumpModeValue
{
    No,
    Tool,
    Auto,
}

public enum NewDayActionValue
{
    None,
    Pause,
    Speed1,
}