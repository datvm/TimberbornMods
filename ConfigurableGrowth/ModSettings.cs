using ModSettings.Core;
using Timberborn.Modding;
using Timberborn.SettingsSystem;
using Timberborn.SingletonSystem;

namespace ConfigurableGrowth;

public class ModSettings : ModSettingsOwner, IUnloadableSingleton
{

    public static float TreeGrowthRate { get; private set; } = 1;
    public static float CropGrowthRate { get; private set; } = 1;
    public static float OtherGrowthRate { get; private set; } = 1;

    ModSetting<float> treeRate, cropRate, otherRate;

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
                .SetTooltip("Multiplier for tree growth (1 for game default)"));

        cropRate = new ModSetting<float>(
            1,
            ModSettingDescriptor
                .Create("Crop growth rate")
                .SetTooltip("Multiplier for crop growth (1 for game default)"));

        otherRate = new ModSetting<float>(
            1,
            ModSettingDescriptor
                .Create("Other growth rate")
                .SetTooltip("Multiplier for other growth (1 for game default)"));

        AddCustomModSetting(treeRate, nameof(TreeGrowthRate));
        AddCustomModSetting(cropRate, nameof(CropGrowthRate));
        AddCustomModSetting(otherRate, nameof(OtherGrowthRate));

        UpdateValues();
    }

    public void Unload()
    {
        UpdateValues();
    }

    void UpdateValues()
    {
        TreeGrowthRate = treeRate.Value;
        CropGrowthRate = cropRate.Value;
        OtherGrowthRate = otherRate.Value;
    }

}
