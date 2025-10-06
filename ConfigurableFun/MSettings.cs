namespace ConfigurableFun;

public class MSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository)
{

    public override string ModId { get; } = nameof(ConfigurableFun);

    public ModSetting<float> PointMul { get; } = new(Modifiers.Default.PointMul, ModSettingDescriptor
        .CreateLocalized("LV.CFun.PointMul")
        .SetLocalizedTooltip("LV.CFun.PointMulDesc"));

    public ModSetting<float> CapacityMul { get; } = new(Modifiers.Default.CapacityMul, ModSettingDescriptor
        .CreateLocalized("LV.CFun.CapacityMul")
        .SetLocalizedTooltip("LV.CFun.CapacityMulDesc"));

    public ModSetting<int> CapacityAdd { get; } = new(Modifiers.Default.CapacityAdd, ModSettingDescriptor
        .CreateLocalized("LV.CFun.CapacityAdd")
        .SetLocalizedTooltip("LV.CFun.CapacityAddDesc"));

    public ModSetting<float> RangeMul { get; } = new(Modifiers.Default.RangeMul, ModSettingDescriptor
        .CreateLocalized("LV.CFun.RangeMul")
        .SetLocalizedTooltip("LV.CFun.RangeMulDesc"));

    public ModSetting<int> RangeAdd { get; } = new(Modifiers.Default.RangeAdd, ModSettingDescriptor
        .CreateLocalized("LV.CFun.RangeAdd")
        .SetLocalizedTooltip("LV.CFun.RangeAddDesc"));

    public Modifiers Current { get; private set; }

    public override void OnAfterLoad()
    {
        base.OnAfterLoad();

        ModSettingChanged += (_, _) => OnSettingChanged();
        OnSettingChanged();
    }

    private void OnSettingChanged()
    {
        Current = new (
            PointMul.Value,
            CapacityMul.Value,
            CapacityAdd.Value,
            RangeMul.Value,
            RangeAdd.Value);
    }

    public readonly record struct Modifiers(float PointMul, float CapacityMul, int CapacityAdd, float RangeMul, int RangeAdd)
    {
        public static readonly Modifiers Default = new(1f, 1f, 0, 1f, 0);

        public bool IsDefault => this == Default;
        public bool PointChanged => PointMul != Default.PointMul;
        public bool CapacityChanged => CapacityMul != Default.CapacityMul || CapacityAdd != Default.CapacityAdd;
        public bool RangeChanged => RangeMul != Default.RangeMul || RangeAdd != Default.RangeAdd;
    }

}
