namespace TImprove.Settings;

public class MSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository), IUnloadableSingleton
{
    public static MSettings? Instance { get; private set; }

    public static readonly ImmutableArray<DayStage> Lights = [.. Enum.GetValues(typeof(DayStage)).Cast<DayStage>().OrderBy(q => q)];

    public override string ModId { get; } = nameof(TImprove);
    public override ModSettingsContext ChangeableOn => ModSettingsContext.All;

    public ModSetting<bool> EnableFreeCamera { get; } = Create(nameof(EnableFreeCamera));
    public ModSetting<bool> FreeCameraLockAngle { get; } = Create(nameof(FreeCameraLockAngle));
    public ModSetting<bool> DisableFog { get; } = Create(nameof(DisableFog));
    public LimitedStringModSetting StaticLight { get; } = new(0,
        [
            new("", "LV.TI.LightNone"),
            .. Lights.Select(q => new LimitedStringModSettingValue(q.ToString(), $"LV.TI.Light{q}"))
        ],
        CreateDescriptor(nameof(StaticLight)));
    public ModSetting<bool> PrioritizeRubbles { get; } = Create(nameof(PrioritizeRubbles));
    public ModSetting<bool> ShowCoords { get; } = Create(nameof(ShowCoords));
    public ModSetting<bool> OnlyShowHeight { get; } = Create(nameof(OnlyShowHeight));
    public ModSetting<bool> ShowGameTime { get; } = Create(nameof(ShowGameTime), true);
    public ModSetting<bool> AddRealTimeClock { get; } = Create(nameof(AddRealTimeClock));
    public ModSetting<bool> Clock24 { get; } = Create(nameof(Clock24));
    public ModSetting<bool> AutoClearDeadTrees { get; } = Create(nameof(AutoClearDeadTrees));
    public ModSetting<bool> EnableSpeedS25 { get; } = Create(nameof(EnableSpeedS25));
    public ModSetting<bool> EnableSpeed4 { get; } = Create(nameof(EnableSpeed4), true);
    public ModSetting<bool> EnableSpeed5 { get; } = Create(nameof(EnableSpeed5));
    public ModSetting<bool> QuickQuit { get; } = Create(nameof(QuickQuit));

    public RangeIntModSetting BiggerBuildDragArea { get; } = new(0, 0, 20, CreateDescriptor(nameof(BiggerBuildDragArea)));
    public static int BiggerBuildDragAreaValue { get; private set; }

    public DayStage? AllDayStage { get; private set; }

    public override void OnBeforeLoad()
    {
        Instance = this;

        OnlyShowHeight.Descriptor.SetEnableCondition(() => ShowCoords.Value);
        Clock24.Descriptor.SetEnableCondition(() => AddRealTimeClock.Value);
    }

    public override void OnAfterLoad()
    {
        StaticLight.ValueChanged += (_, _) => OnStaticLightChanged();
        OnStaticLightChanged();

        BiggerBuildDragArea.ValueChanged += (_, e) => BiggerBuildDragAreaValue = e;
        BiggerBuildDragAreaValue = BiggerBuildDragArea.Value;
    }

    void OnStaticLightChanged()
    {
        var e = StaticLight.Value;
        AllDayStage = e == "" ? null : Enum.Parse<DayStage>(e);
    }

    public void Unload()
    {
        Instance = null;
    }

    static ModSettingDescriptor CreateDescriptor(string name) =>
        ModSettingDescriptor
            .CreateLocalized("LV.TI." + name)
            .SetLocalizedTooltip("LV.TI." + name + "Desc");

    static ModSetting<bool> Create(string name, bool defaultValue = false) =>
        new(defaultValue, CreateDescriptor(name));

}

public enum NewDayActionValue
{
    None,
    Pause,
    Speed1,
}