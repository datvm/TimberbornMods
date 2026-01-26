
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
    public static bool PlatformOver1x1 { get; private set; } = true;
    public static bool MagicStructure { get; private set; } = false;
    public static bool HangingStructure { get; private set; } = false;
    public static bool SuperHangingTerrain { get; private set; } = false;
    public static int SuperHangingTerrainLimit { get; private set; } = 6;
    public static bool NoBottomOfMap { get; private set; } = false;

    public static bool ModifyObjects
        => RemoveGroundOnly
        || RemoveRoofOnly
        || AllowFlooded
        || AlwaysSolid
        || SuperStructure
        || PlatformOver1x1
        || NoBottomOfMap;

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

    readonly ModSetting<bool> platformOver1x1 = new(
        true,
        ModSettingDescriptor.CreateLocalized("LV.NBR.PlatformOver1x1")
            .SetLocalizedTooltip("LV.NBR.PlatformOver1x1Desc"));

    readonly ModSetting<bool> magicStructure = new(
        false,
        ModSettingDescriptor.CreateLocalized("LV.NBR.MagicStructure")
            .SetLocalizedTooltip("LV.NBR.MagicStructureDesc"));

    readonly ModSetting<bool> hangingStructure = new(
        false,
        ModSettingDescriptor.CreateLocalized("LV.NBR.HangingStructure")
            .SetLocalizedTooltip("LV.NBR.HangingStructureDesc"));

    readonly ModSetting<bool> superHangingTerrain = new(
        false,
        ModSettingDescriptor.CreateLocalized("LV.NBR.SuperHangingTerrain")
            .SetLocalizedTooltip("LV.NBR.SuperHangingTerrainDesc"));

    readonly RangeIntModSetting superHangingTerrainLimit = new(
        6, 3, 50,
        ModSettingDescriptor.CreateLocalized("LV.NBR.SuperHangingTerrainLimit")
            .SetLocalizedTooltip("LV.NBR.SuperHangingTerrainLimitDesc"));

    readonly ModSetting<bool> noBottomOfMap = new(
        false,
        ModSettingDescriptor.CreateLocalized("LV.NBR.NoBottomOfMap")
            .SetLocalizedTooltip("LV.NBR.NoBottomOfMapDesc"));

    public override void OnAfterLoad()
    {
        magicStructure.Descriptor.SetEnableCondition(() => superStructure.Value);
        hangingStructure.Descriptor.SetEnableCondition(() => superStructure.Value && magicStructure.Value);
        superHangingTerrainLimit.Descriptor.SetEnableCondition(() => superHangingTerrain.Value);

        AddCustomModSetting(removeGroundOnly, nameof(removeGroundOnly));
        AddCustomModSetting(removeRoofOnly, nameof(removeRoofOnly));
        AddCustomModSetting(platformOver1x1, nameof(platformOver1x1));
        AddCustomModSetting(alwaysSolid, nameof(alwaysSolid));
        AddCustomModSetting(allowFlooded, nameof(allowFlooded));
        AddCustomModSetting(superStructure, nameof(superStructure));
        AddCustomModSetting(magicStructure, nameof(magicStructure));
        AddCustomModSetting(hangingStructure, nameof(hangingStructure));
        AddCustomModSetting(superHangingTerrain, nameof(superHangingTerrain));
        AddCustomModSetting(superHangingTerrainLimit, nameof(superHangingTerrainLimit));
        AddCustomModSetting(noBottomOfMap, nameof(noBottomOfMap));

        UpdateValues();
    }

    void UpdateValues()
    {
        RemoveGroundOnly = removeGroundOnly.Value;
        RemoveRoofOnly = removeRoofOnly.Value;
        AllowFlooded = allowFlooded.Value;
        AlwaysSolid = alwaysSolid.Value;
        SuperStructure = superStructure.Value;
        PlatformOver1x1 = platformOver1x1.Value;
        MagicStructure = magicStructure.Value;
        HangingStructure = hangingStructure.Value;
        SuperHangingTerrain = superHangingTerrain.Value;
        NoBottomOfMap = noBottomOfMap.Value;
        SuperHangingTerrainLimit = superHangingTerrainLimit.Value;

        if (SuperHangingTerrain)
        {
            HangingTerrainPatches.PatchHangingTerrain();
        }
    }

    public void Unload()
    {
        UpdateValues();
    }
}
