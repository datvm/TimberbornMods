namespace NoWaterCompression;

public class MSettings(
    ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository,
    IModSettingsContextProvider ctx
) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository), IUnloadableSingleton
{

    public override string ModId => nameof(NoWaterCompression);
    public override ModSettingsContext ChangeableOn => ModSettingsContext.All;

    readonly ModSetting<bool> noCompression = new(true, ModSettingDescriptor
        .CreateLocalized("LV.NWC.NoCompression")
        .SetLocalizedTooltip("LV.NWC.NoCompressionDesc"));

    readonly ModSetting<bool> freeFlow = new(false, ModSettingDescriptor
        .CreateLocalized("LV.NWC.FreeFlow")
        .SetLocalizedTooltip("LV.NWC.FreeFlowDesc"));

    readonly RangeIntModSetting maxCurrentMultiplier = new(1, 1, 10, ModSettingDescriptor
        .CreateLocalized("LV.NWC.MaxCurrentMultiplier")
        .SetLocalizedTooltip("LV.NWC.MaxCurrentMultiplierDesc"));

    public event Action OnValueChanged = delegate { };

    public static bool NoCompression { get; private set; } = true;
    public static bool FreeFlow { get; private set; } = true;
    public static int MaxCurrentMultiplier { get; private set; } = 1;

    public override void OnAfterLoad()
    {
        freeFlow.Descriptor.SetEnableCondition(() => ctx.Context == ModSettingsContext.MainMenu);

        AddCustomModSetting(noCompression, nameof(noCompression));
        AddCustomModSetting(freeFlow, nameof(freeFlow));
        AddCustomModSetting(maxCurrentMultiplier, nameof(maxCurrentMultiplier));

        noCompression.ValueChanged += (_, _) => InternalOnValueChanged();
        freeFlow.ValueChanged += (_, _) => InternalOnValueChanged();
        maxCurrentMultiplier.ValueChanged += (_, _) => InternalOnValueChanged();
        InternalOnValueChanged();
    }

    void InternalOnValueChanged()
    {
        NoCompression = noCompression.Value;
        FreeFlow = freeFlow.Value;
        MaxCurrentMultiplier = maxCurrentMultiplier.Value;

        OnValueChanged();
    }

    public void Unload()
    {
        InternalOnValueChanged();
    }

}
