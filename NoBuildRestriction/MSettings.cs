namespace NoBuildRestriction;

public class MSettings(
    ISettings settings,
    ModSettingsOwnerRegistry modSettingsOwnerRegistry,
    ModRepository modRepository
) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository), IUnloadableSingleton
{

    public static bool RemoveGroundOnly { get; private set; } = true;
    public static bool RemoveRoofOnly { get; private set; } = true;
    public static bool AllowFlooded { get; private set; } = false;
    public static bool AlwaysSolid { get; private set; } = false;
    public static bool SuperStructure { get; private set; } = false;

    public override string ModId { get; } = nameof(NoBuildRestriction);

    readonly ModSetting<bool> removeGroundOnly = new(
        true,
        ModSettingDescriptor.CreateLocalized("LV.NBR.RemoveGroundOnly")
            .SetLocalizedTooltip("LV.NBR.RemoveGroundOnlyDesc"));

    readonly ModSetting<bool> removeRoofOnly = new(
        true,
        ModSettingDescriptor.CreateLocalized("LV.NBR.RemoveRoofOnly")
            .SetLocalizedTooltip("LV.NBR.RemoveRoofOnlyDesc"));

    readonly ModSetting<bool> allowFlooded = new(
        false,
        ModSettingDescriptor.CreateLocalized("LV.NBR.AllowFlooded")
            .SetLocalizedTooltip("LV.NBR.AllowFloodedDesc"));

    readonly ModSetting<bool> alwaysSolid = new(
        false,
        ModSettingDescriptor.CreateLocalized("LV.NBR.AlwaysSolid")
            .SetLocalizedTooltip("LV.NBR.AlwaysSolidDesc"));

    readonly ModSetting<bool> superStructure = new(
        false,
        ModSettingDescriptor.CreateLocalized("LV.NBR.SuperStructure")
            .SetLocalizedTooltip("LV.NBR.SuperStructureDesc"));

    public override void OnAfterLoad()
    {
        AddCustomModSetting(removeGroundOnly, nameof(removeGroundOnly));
        AddCustomModSetting(removeRoofOnly, nameof(removeRoofOnly));
        AddCustomModSetting(alwaysSolid, nameof(alwaysSolid));
        AddCustomModSetting(allowFlooded, nameof(allowFlooded));
        AddCustomModSetting(superStructure, nameof(superStructure));

        UpdateValues();
    }

    void UpdateValues()
    {
        RemoveGroundOnly = removeGroundOnly.Value;
        RemoveRoofOnly = removeRoofOnly.Value;
        AllowFlooded = allowFlooded.Value;
        AlwaysSolid = alwaysSolid.Value;
        SuperStructure = superStructure.Value;
    }

    public void Unload()
    {
        UpdateValues();
    }
}
