
namespace ConfigurableBuildingRange;

public class MSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository) 
    : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository), IUnloadableSingleton
{
    
    public static int ResourceBuildingsRangeValue { get; private set; }
    public static int DistrictTerrainRangeValue { get; private set; }

    public override string ModId { get; } = nameof(ConfigurableBuildingRange);

    public ModSetting<int> ResourceBuildingsRange { get; } = new(20, ModSettingDescriptor
        .CreateLocalized("LV.CBldR.ResourceBuildingsRange")
        .SetLocalizedTooltip("LV.CBldR.ResourceBuildingsRangeDesc"));

    public ModSetting<int> DistrictTerrainRange { get; } = new(10, ModSettingDescriptor
        .CreateLocalized("LV.CBldR.DistrictTerrainRange")
        .SetLocalizedTooltip("LV.CBldR.DistrictTerrainRangeDesc"));

    public void Unload()
    {
        ResourceBuildingsRangeValue = ResourceBuildingsRange.Value;
        DistrictTerrainRangeValue = DistrictTerrainRange.Value;
    }
}
