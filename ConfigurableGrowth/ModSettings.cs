namespace ConfigurableGrowth;

public class ModSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository)
    : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository), IUnloadableSingleton
{

    public static float TreeGrowthRate { get; private set; } = 1;
    public static float CropGrowthRate { get; private set; } = 1;
    public static float GatherableGrowthRate { get; private set; } = 1;
    public static int SpreadDistance { get; private set; } = 1;
    public static bool SpreadVertically { get; private set; } = false;
    public static float ReproducibleChanceMultiplier { get; private set; } = 1;

    readonly ModSetting<float> treeRate = new(
        2,
        ModSettingDescriptor
            .CreateLocalized("CG.TreeGrowthRate")
            .SetLocalizedTooltip("CG.TreeGrowthRateDesc"));

    readonly ModSetting<float> cropRate = new(
        1,
        ModSettingDescriptor
            .CreateLocalized("CG.CropGrowthRate")
            .SetLocalizedTooltip("CG.CropGrowthRateDesc"));

    readonly ModSetting<float> gatherableRate = new(
        1,
        ModSettingDescriptor
            .CreateLocalized("CG.GatherableGrowthRate")
            .SetLocalizedTooltip("CG.GatherableGrowthRateDesc"));

    readonly ModSetting<float> reproducibleChanceMultiplier = new(
        1,
        ModSettingDescriptor
            .CreateLocalized("CG.ReproducibleChanceMultiplier")
            .SetLocalizedTooltip("CG.ReproducibleChanceMultiplierDesc"));

    readonly ModSetting<int> spreadDistance = new(
        1,
        ModSettingDescriptor
            .CreateLocalized("CG.SpreadDistance")
            .SetLocalizedTooltip("CG.SpreadDistanceDesc"));

    readonly ModSetting<bool> spreadVertically = new(
        false,
        ModSettingDescriptor
            .CreateLocalized("CG.SpreadVertically")
            .SetLocalizedTooltip("CG.SpreadVerticallyDesc"));

    public override string ModId => nameof(ConfigurableGrowth);

    public override void OnAfterLoad()
    {
        AddCustomModSetting(treeRate, nameof(TreeGrowthRate));
        AddCustomModSetting(cropRate, nameof(CropGrowthRate));
        AddCustomModSetting(gatherableRate, nameof(GatherableGrowthRate));
        AddCustomModSetting(reproducibleChanceMultiplier, nameof(ReproducibleChanceMultiplier));
        AddCustomModSetting(spreadDistance, nameof(SpreadDistance));
        AddCustomModSetting(spreadVertically, nameof(SpreadVertically));

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
        GatherableGrowthRate = gatherableRate.Value;
        SpreadDistance = spreadDistance.Value;
        SpreadVertically = spreadVertically.Value;
        ReproducibleChanceMultiplier = reproducibleChanceMultiplier.Value;
    }

}
