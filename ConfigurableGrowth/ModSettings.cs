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
                .CreateLocalized("CG.TreeGrowthRate")
                .SetLocalizedTooltip("CG.TreeGrowthRateDesc"));

        cropRate = new ModSetting<float>(
            1,
            ModSettingDescriptor
                .CreateLocalized("CG.CropGrowthRate")
                .SetLocalizedTooltip("CG.CropGrowthRateDesc"));

        gatherableRate = new ModSetting<float>(
            1,
            ModSettingDescriptor
                .CreateLocalized("CG.GatherableGrowthRate")
                .SetLocalizedTooltip("CG.GatherableGrowthRateDesc"));

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
