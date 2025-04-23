namespace ConfigurableWorkplace;

public class MSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository), IUnloadableSingleton
{
    public static bool BuildingShift { get; private set; } = true;
    public static float MaxWorkerMul { get; private set; } = 1f;
    public static bool BotEverywhere { get; private set; } = false;

    public override string ModId { get; } = nameof(ConfigurableWorkplace);

    readonly ModSetting<bool> buildingShift = new(true, ModSettingDescriptor
        .CreateLocalized("LV.CWk.BuildingShift")
        .SetLocalizedTooltip("LV.CWk.BuildingShiftDesc"));

    readonly ModSetting<float> maxWorkerMul = new(1f, ModSettingDescriptor
        .CreateLocalized("LV.CWk.MaxWorkerMul")
        .SetLocalizedTooltip("LV.CWk.MaxWorkerMulDesc"));

    readonly ModSetting<bool> botEverywhere = new(false, ModSettingDescriptor
        .CreateLocalized("LV.CWk.BotEverywhere")
        .SetLocalizedTooltip("LV.CWk.BotEverywhereDesc"));

    public override void OnAfterLoad()
    {
        AddCustomModSetting(buildingShift, nameof(buildingShift));
        AddCustomModSetting(maxWorkerMul, nameof(maxWorkerMul));
        AddCustomModSetting(botEverywhere, nameof(botEverywhere));

        UpdateValues();
    }

    void UpdateValues()
    {
        BuildingShift = buildingShift.Value;
        MaxWorkerMul = maxWorkerMul.Value;
        BotEverywhere = botEverywhere.Value;
    }

    public void Unload()
    {
        UpdateValues();
    }
}
