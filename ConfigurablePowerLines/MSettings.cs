namespace ConfigurablePowerLines;

public class MSettings(
    ISettings settings,
    ModSettingsOwnerRegistry modSettingsOwnerRegistry,
    ModRepository modRepository
) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository)
{

    public override string ModId { get; } = nameof(ConfigurablePowerLines);
    public override ModSettingsContext ChangeableOn { get; } = ModSettingsContext.All;

    public static int MaxConnectionsValue { get; private set; }
    public static int MaxDistanceValue { get; private set; }
    public static int MaxInclinationValue { get; private set; }
    public static bool PowerlineThroughObstaclesValue { get; private set; }

    public ModSetting<int> MaxConnections { get; } = new(4, ModSettingDescriptor
        .CreateLocalized("LV.CPL.MaxConnections")
        .SetLocalizedTooltip("LV.CPL.MaxConnectionsDesc"));
    public ModSetting<int> MaxDistance { get; } = new(100, ModSettingDescriptor
        .CreateLocalized("LV.CPL.MaxDistance")
        .SetLocalizedTooltip("LV.CPL.MaxDistanceDesc"));
    public ModSetting<int> MaxInclination { get; } = new(75, ModSettingDescriptor
        .CreateLocalized("LV.CPL.MaxInclination")
        .SetLocalizedTooltip("LV.CPL.MaxInclinationDesc"));
    public ModSetting<bool> PowerlineThroughObstacles { get; } = new(false, ModSettingDescriptor
        .CreateLocalized("LV.CPL.PowerlineThroughObstacles")
        .SetLocalizedTooltip("LV.CPL.PowerlineThroughObstaclesDesc"));

    public override void OnAfterLoad()
    {
        ModSettingChanged += (_, _) => UpdateValues();
        UpdateValues();
    }

    void UpdateValues()
    {
        MaxConnectionsValue = MaxConnections.Value;
        MaxDistanceValue = MaxDistance.Value;
        MaxInclinationValue = MaxInclination.Value;
        PowerlineThroughObstaclesValue = PowerlineThroughObstacles.Value;
    }

}
