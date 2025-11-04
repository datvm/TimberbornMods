namespace ConfigurableTubeZipLine;

public class MSettings(
    ISettings settings,
    ModSettingsOwnerRegistry modSettingsOwnerRegistry,
    ModRepository modRepository
) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository), IUnloadableSingleton
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

    readonly ModSetting<int> tubewaySpeed = CreateIntModSettings("TubewaySpeed", 600);
    readonly ModSetting<int> ziplineSpeed = CreateIntModSettings("ZiplineSpeed", 300);
    readonly ModSetting<int> ziplineMaxConnection = CreateIntModSettings("ZiplineSpeedMaxCon", 4);
    readonly ModSetting<int> ziplineMaxDistance = CreateIntModSettings("ZiplineSpeedMaxDistance", 30);
    readonly ModSetting<int> ziplineMaxInclination = CreateIntModSettings("ZiplineSpeedMaxInclination", 50);
    readonly ModSetting<bool> ziplineForIronTeeth = CreateBoolModSettings("ZiplineForIronTeeth");
    readonly ModSetting<bool> tubewayForFolktails = CreateBoolModSettings("TubewayForFolktails");
    readonly ModSetting<bool> noWaterPenalty = CreateBoolModSettings("NoWaterPenalty");
    readonly ModSetting<bool> ziplineThroughObstacles = CreateBoolModSettings("ZiplineThroughObstacles");

    static ModSetting<bool> CreateBoolModSettings(string locName)
    {
        return new(
            false,
            ModSettingDescriptor
                .CreateLocalized("LV.CTZ." + locName)
                .SetLocalizedTooltip($"LV.CTZ.{locName}Desc"));
    }

    static ModSetting<int> CreateIntModSettings(string locName, int defaultValue)
    {
        return new(
            defaultValue,
            ModSettingDescriptor
                .CreateLocalized("LV.CTZ." + locName)
                .SetLocalizedTooltip($"LV.CTZ.{locName}Desc"));
    }

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

    void UpdateValues()
    {
        TubewaySpeed = tubewaySpeed.Value;

        ZiplineForIronTeeth = ziplineForIronTeeth.Value;
        TubewayForFolktails = tubewayForFolktails.Value;
        NoWaterPenalty = noWaterPenalty.Value;


        ZiplineSpeed = ziplineSpeed.Value;
        ZiplineMaxConnection = ziplineMaxConnection.Value;
        ZiplineMaxDistance = ziplineMaxDistance.Value;
        ZiplineMaxInclination = ziplineMaxInclination.Value;
        ZiplineThroughObstacles = ziplineThroughObstacles.Value;
    }

    public void Unload()
    {
        UpdateValues();
    }
}
