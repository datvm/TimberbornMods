namespace TheArchitectsToolkit;

public class MSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository)
{

    protected override string ModId => nameof(TheArchitectsToolkit);
    public override ModSettingsContext ChangeableOn => ModSettingsContext.MainMenu;

    readonly ModSetting<bool> unlimitedMapSize = new(true, ModSettingDescriptor
        .CreateLocalized("LV.TAT.UnlimitedMapSize")
        .SetLocalizedTooltip("LV.TAT.UnlimitedMapSizeDesc"));
    readonly ModSetting<bool> gameToMap = new(true, ModSettingDescriptor
        .CreateLocalized("LV.TAT.GameToMap")
        .SetLocalizedTooltip("LV.TAT.GameToMapDesc"));
    readonly ModSetting<bool> lockOnSaveMap = new(true, ModSettingDescriptor
        .CreateLocalized("LV.TAT.LockOnSaveMap")
        .SetLocalizedTooltip("LV.TAT.LockOnSaveMapDesc")
        .SetEnableCondition(() => GameToMap));

    public static bool UnlimitedMapSize { get; private set; }
    public static bool GameToMap { get; private set; }
    public static bool LockOnSaveMap { get; private set; }

    public event Action OnSettingsChanged = delegate { };

    protected override void OnAfterLoad()
    {
        AddCustomModSetting(unlimitedMapSize, nameof(unlimitedMapSize));
        AddCustomModSetting(gameToMap, nameof(gameToMap));
        AddCustomModSetting(lockOnSaveMap, nameof(lockOnSaveMap));

        unlimitedMapSize.ValueChanged += (_, _) => InternalOnSettingsChanged();
        gameToMap.ValueChanged += (_, _) => InternalOnSettingsChanged();
        lockOnSaveMap.ValueChanged += (_, _) => InternalOnSettingsChanged();

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
    }

}