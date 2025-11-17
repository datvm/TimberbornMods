namespace ConfigurablePlants;

public class MSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository)
{
    static readonly ReadonlyTextModSetting.TextSettings Section = new(TextAnchor.MiddleCenter);

    public override string ModId { get; } = nameof(ConfigurablePlants);

    public static bool WithoutGroundValue { get; private set; }
    public static bool NoConUpValue { get; private set; }

    public ReadonlyTextModSetting PlacementSection { get; } = new(ModSettingDescriptor.CreateLocalized("LV.CPl.PlacementSect"), Section);

    public ModSetting<bool> RemoveCorner { get; } = CreateB(nameof(RemoveCorner));
    public ModSetting<bool> RemovePath { get; } = CreateB(nameof(RemovePath));
    public ModSetting<bool> WithoutGround { get; } = CreateB(nameof(WithoutGround), false);
    public ModSetting<bool> NoConUp { get; } = CreateB(nameof(NoConUp), false);

    public ReadonlyTextModSetting GrowthSection { get; } = new(ModSettingDescriptor.CreateLocalized("LV.CPl.GrowthSect"), Section);

    public ModSetting<float> TreeGrowthRate { get; } = CreateF(nameof(TreeGrowthRate));
    public ModSetting<float> CropGrowthRate { get; } = CreateF(nameof(CropGrowthRate));
    public ModSetting<float> GatherableGrowthRate { get; } = CreateF(nameof(GatherableGrowthRate));
    
    public ModSetting<float> TreeOutputMul { get; } = CreateF(nameof(TreeOutputMul));
    public ModSetting<float> CropOutputMul { get; } = CreateF(nameof(CropOutputMul));
    public ModSetting<float> GatherableOutputMul { get; } = CreateF(nameof(GatherableOutputMul));

    public ModSetting<float> ReproducibleChanceMultiplier { get; } = CreateF(nameof(ReproducibleChanceMultiplier));

    static ModSetting<bool> CreateB(string key, bool defaultValue = true)
        => new(defaultValue, ModSettingDescriptor
            .CreateLocalized("LV.CPl." + key)
            .SetLocalizedTooltip("LV.CPl." + key + "Desc"));

    static ModSetting<float> CreateF(string key, float defaultValue = 1f)
        => new(defaultValue, ModSettingDescriptor
            .CreateLocalized("LV.CPl." + key)
            .SetLocalizedTooltip("LV.CPl." + key + "Desc"));

    public override void OnAfterLoad()
    {
        base.OnAfterLoad();

        NoConUp.Descriptor.SetEnableCondition(() => WithoutGround.Value);

        WithoutGround.ValueChanged += (_, v) => WithoutGroundValue = v;
        WithoutGroundValue = WithoutGround.Value;

        NoConUp.ValueChanged += (_, v) => NoConUpValue = v;
        NoConUpValue = NoConUp.Value;
    }

}
