namespace TheArchitectsToolkit;

public class MSettings(
    ISettings settings,
    ModSettingsOwnerRegistry modSettingsOwnerRegistry,
    ModRepository modRepository,
    IModSettingsContextProvider ctx
) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository)
{

    public override string ModId => nameof(TheArchitectsToolkit);
    public override ModSettingsContext ChangeableOn => ModSettingsContext.All;

    readonly ModSetting<bool> unlimitedMapSize = new(true, ModSettingDescriptor
        .CreateLocalized("LV.TAT.UnlimitedMapSize")
        .SetLocalizedTooltip("LV.TAT.UnlimitedMapSizeDesc")
        .SetEnableCondition(() => ctx.Context == ModSettingsContext.MainMenu));
    readonly ModSetting<bool> gameToMap = new(true, ModSettingDescriptor
        .CreateLocalized("LV.TAT.GameToMap")
        .SetLocalizedTooltip("LV.TAT.GameToMapDesc")
        .SetEnableCondition(() => ctx.Context == ModSettingsContext.MainMenu));
    readonly ModSetting<bool> lockOnSaveMap = new(true, ModSettingDescriptor
        .CreateLocalized("LV.TAT.LockOnSaveMap")
        .SetLocalizedTooltip("LV.TAT.LockOnSaveMapDesc")
        .SetEnableCondition(() => GameToMap)
        .SetEnableCondition(() => ctx.Context == ModSettingsContext.MainMenu));
    readonly RangeIntModSetting defaultWaterSourceStrength = new(1, -10, 10, ModSettingDescriptor
        .CreateLocalized("LV.TAT.DefaultWaterSourceStrength")
        .SetLocalizedTooltip("LV.TAT.DefaultWaterSourceStrengthDesc"));

    public static bool UnlimitedMapSize { get; private set; }
    public static bool GameToMap { get; private set; }
    public static bool LockOnSaveMap { get; private set; }
    public static int DefaultWaterSourceStrength { get; private set; }

    public event Action OnSettingsChanged = delegate { };

    public override void OnAfterLoad()
    {
        AddCustomModSetting(unlimitedMapSize, nameof(unlimitedMapSize));
        AddCustomModSetting(gameToMap, nameof(gameToMap));
        AddCustomModSetting(lockOnSaveMap, nameof(lockOnSaveMap));
        AddCustomModSetting(defaultWaterSourceStrength, nameof(defaultWaterSourceStrength));

        unlimitedMapSize.ValueChanged += (_, _) => InternalOnSettingsChanged();
        gameToMap.ValueChanged += (_, _) => InternalOnSettingsChanged();
        lockOnSaveMap.ValueChanged += (_, _) => InternalOnSettingsChanged();
        defaultWaterSourceStrength.ValueChanged += (_, _) => InternalOnSettingsChanged();

        InternalOnSettingsChanged();
    }

    void InternalOnSettingsChanged()
    {
        UpdateValues();
        OnSettingsChanged();
    }

    void UpdateValues()
    {
        UnlimitedMapSize = unlimitedMapSize.Value;
        GameToMap = gameToMap.Value;
        LockOnSaveMap = lockOnSaveMap.Value;
        DefaultWaterSourceStrength = defaultWaterSourceStrength.Value;
    }

}