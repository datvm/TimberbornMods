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


    ModSetting<float>? treeRate, cropRate, gatherableRate;


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
                .SetTooltip("Multiplier for Tree growth time like Pines or Oaks (1 for game default)"));

        cropRate = new ModSetting<float>(
            1,
            ModSettingDescriptor
                .Create("Crop growth rate")
                .SetTooltip("Multiplier for Crop growth time like Carrots or Potatoes (1 for game default)"));

        gatherableRate = new ModSetting<float>(
            1,
            ModSettingDescriptor
                .Create("Product growth rate")
                .SetTooltip("Multiplier for products growth time like Pine Resin or Berries (1 for game default)"));

        AddCustomModSetting(treeRate, nameof(TreeGrowthRate));
        AddCustomModSetting(cropRate, nameof(CropGrowthRate));
        AddCustomModSetting(gatherableRate, nameof(GatherableGrowthRate));

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
    }

}
