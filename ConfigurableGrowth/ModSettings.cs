using ModSettings.Core;
using Timberborn.Modding;
using Timberborn.SettingsSystem;
using Timberborn.SingletonSystem;

namespace ConfigurableGrowth;

public class ModSettings : ModSettingsOwner, IUnloadableSingleton
{

    public static float TreeGrowthRate { get; private set; } = 1;
    public static float CropGrowthRate { get; private set; } = 1;
    public static float GatherableGrowthRate { get; private set; } = 1;
    public static float OtherGrowthRate { get; private set; } = 1;

    ModSetting<float>? treeRate, cropRate, gatherableRate, otherRate;

    public ModSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository) : base(settings, modSettingsOwnerRegistry, modRepository)
    {
    }

    protected override string ModId => nameof(ConfigurableGrowth);

    protected override void OnAfterLoad()
    {
        treeRate = new ModSetting<float>(
            2,
            ModSettingDescriptor
                .Create("Tree growth rate")
                .SetTooltip("Multiplier for tree growth like Pines or Oaks (1 for game default)"));

        cropRate = new ModSetting<float>(
            1,
            ModSettingDescriptor
                .Create("Crop growth rate")
                .SetTooltip("Multiplier for crop growth like Carrots or Potatoes (1 for game default)"));

        gatherableRate = new ModSetting<float>(
            1,
            ModSettingDescriptor
                .Create("Gatherable growth rate")
                .SetTooltip("Multiplier for gatherable growth like Pine Resin or Berries (1 for game default)"));

        otherRate = new ModSetting<float>(
            1,
            ModSettingDescriptor
                .Create("Other growth rate")
                .SetTooltip("Multiplier for other growth, currently unused in unmodded games (1 for game default)"));

        AddCustomModSetting(treeRate, nameof(TreeGrowthRate));
        AddCustomModSetting(cropRate, nameof(CropGrowthRate));
        AddCustomModSetting(gatherableRate, nameof(GatherableGrowthRate));
        AddCustomModSetting(otherRate, nameof(OtherGrowthRate));

        UpdateValues();
    }

    public void Unload()
    {
        UpdateValues();
    }

    void UpdateValues()
    {
        TreeGrowthRate = treeRate?.Value ?? 1;
        CropGrowthRate = cropRate?.Value ?? 1;
        GatherableGrowthRate = gatherableRate?.Value ?? 1;
        OtherGrowthRate = otherRate?.Value ?? 1;
    }

}
