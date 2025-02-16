namespace ConfigurableTubeZipLine;

public class MSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository)
    : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository), IUnloadableSingleton
{
    public static int TubewaySpeed { get; private set; }
    public static int ZiplineSpeed { get; private set; }
    public static int ZiplineMaxConnection { get; private set; }
    public static int ZiplineMaxDistance { get; private set; }
    public static int ZiplineMaxInclination { get; private set; }
    public static bool ZiplineForIronTeeth { get; private set; }
    public static bool TubewayForFolktails { get; private set; }
    public static bool NoWaterPenalty { get; private set; }
    public static bool ZiplineThroughObstacles { get; private set; }

    #region ModSettings

    readonly ModSetting<int> tubewaySpeed = new(
        600,
        ModSettingDescriptor
            .CreateLocalized("LV.CTZ.TubewaySpeed")
            .SetLocalizedTooltip("LV.CTZ.TubewaySpeedDesc"));

    readonly ModSetting<int> ziplineSpeed = new(
        300,
        ModSettingDescriptor
            .CreateLocalized("LV.CTZ.ZiplineSpeed")
            .SetLocalizedTooltip("LV.CTZ.ZiplineSpeedDesc"));

    readonly ModSetting<int> ziplineMaxConnection = new(
        4,
        ModSettingDescriptor
            .CreateLocalized("LV.CTZ.ZiplineSpeedMaxCon")
            .SetLocalizedTooltip("LV.CTZ.ZiplineSpeedMaxConDesc"));

    readonly ModSetting<int> ziplineMaxDistance = new(
        30,
        ModSettingDescriptor
            .CreateLocalized("LV.CTZ.ZiplineSpeedMaxDistance")
            .SetLocalizedTooltip("LV.CTZ.ZiplineSpeedMaxDistanceDesc"));

    readonly ModSetting<int> ziplineMaxInclination = new(
        50,
        ModSettingDescriptor
            .CreateLocalized("LV.CTZ.ZiplineSpeedMaxInclination")
            .SetLocalizedTooltip("LV.CTZ.ZiplineSpeedMaxInclinationDesc"));

    readonly ModSetting<bool> ziplineForIronTeeth = new(
        false,
        ModSettingDescriptor
            .CreateLocalized("LV.CTZ.ZiplineForIronTeeth")
            .SetLocalizedTooltip("LV.CTZ.ZiplineForIronTeethDesc"));

    readonly ModSetting<bool> tubewayForFolktails = new(
        false,
        ModSettingDescriptor
            .CreateLocalized("LV.CTZ.TubewayForFolktails")
            .SetLocalizedTooltip("LV.CTZ.TubewayForFolktailsDesc"));

    readonly ModSetting<bool> noWaterPenalty = new(
        false,
        ModSettingDescriptor
            .CreateLocalized("LV.CTZ.NoWaterPenalty")
            .SetLocalizedTooltip("LV.CTZ.NoWaterPenaltyDesc"));

    readonly ModSetting<bool> ziplineThroughObstacles = new(
        false,
        ModSettingDescriptor
            .CreateLocalized("LV.CTZ.ZiplineThroughObstacles")
            .SetLocalizedTooltip("LV.CTZ.ZiplineThroughObstaclesDesc"));

    #endregion

    public override string ModId => nameof(ConfigurableTubeZipLine);

    public override void OnAfterLoad()
    {
        AddCustomModSetting(tubewaySpeed, nameof(TubewaySpeed));
        AddCustomModSetting(ziplineSpeed, nameof(ZiplineSpeed));
        AddCustomModSetting(ziplineMaxConnection, nameof(ZiplineMaxConnection));
        AddCustomModSetting(ziplineMaxDistance, nameof(ZiplineMaxDistance));
        AddCustomModSetting(ziplineMaxInclination, nameof(ZiplineMaxInclination));
        AddCustomModSetting(ziplineThroughObstacles, nameof(ZiplineThroughObstacles));
        AddCustomModSetting(ziplineForIronTeeth, nameof(ZiplineForIronTeeth));
        AddCustomModSetting(tubewayForFolktails, nameof(TubewayForFolktails));
        AddCustomModSetting(noWaterPenalty, nameof(NoWaterPenalty));

        UpdateValues();
    }

    static readonly FieldInfo cableUnitCostField = typeof(ZiplineCableNavMesh).Field(nameof(ZiplineCableNavMesh.CableUnitCost));
    void OnZiplineSpeedChanged()
    {
        cableUnitCostField.SetValue(null, CalculateCost(ZiplineSpeed));
    }

    void UpdateValues()
    {
        TubewaySpeed = tubewaySpeed.Value;
        ZiplineSpeed = ziplineSpeed.Value;
        ZiplineMaxConnection = ziplineMaxConnection.Value;
        ZiplineMaxDistance = ziplineMaxDistance.Value;
        ZiplineMaxInclination = ziplineMaxInclination.Value;
        ZiplineForIronTeeth = ziplineForIronTeeth.Value;
        TubewayForFolktails = tubewayForFolktails.Value;
        NoWaterPenalty = noWaterPenalty.Value;
        ZiplineThroughObstacles = ziplineThroughObstacles.Value;

        OnZiplineSpeedChanged();
    }

    public static float CalculateCost(int bonusSpeed)
    {
        return 1f / ((100 + bonusSpeed) / 100f);
    }

    public void Unload()
    {
        UpdateValues();
    }
}
