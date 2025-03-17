namespace ConfigurableHousing;

public class MSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository), IUnloadableSingleton
{
    public static float MaxBeaverMultiplier { get; private set; } = 1;
    public static float SleepSatisfactionMultiplier { get; private set; } = 1;
    public static float ShelterSatisfactionMultiplier { get; private set; } = 1;
    public static bool MoveEntranceFloor { get; private set; } = false;
    public static bool AddOtherFaction { get; private set; } = false;

    readonly ModSetting<float> maxBeaverMultiplier = new(
        1.5f,
        ModSettingDescriptor
            .CreateLocalized("LV.CH.MaxBeaverMul")
            .SetLocalizedTooltip("LV.CH.MaxBeaverMulDesc"));

    readonly ModSetting<float> sleepSatisfactionMultiplier = new(
        1,
        ModSettingDescriptor
            .CreateLocalized("LV.CH.SleepMult")
            .SetLocalizedTooltip("LV.CH.SleepMultDesc"));

    readonly ModSetting<float> shelterSatisfactionMultiplier = new(
        1,
        ModSettingDescriptor
            .CreateLocalized("LV.CH.ShelterMult")
            .SetLocalizedTooltip("LV.CH.ShelterMultDesc"));

    readonly ModSetting<bool> moveEntranceFloor = new(
        false,
        ModSettingDescriptor
            .CreateLocalized("LV.CH.MoveEntranceFloor")
            .SetLocalizedTooltip("LV.CH.MoveEntranceFloorDesc"));

    readonly ModSetting<bool> addOtherFaction = new(
        false,
        ModSettingDescriptor
            .CreateLocalized("LV.CH.AddOtherFaction")
            .SetLocalizedTooltip("LV.CH.AddOtherFactionDesc"));

    public override string ModId { get; } = nameof(ConfigurableHousing);

    public override void OnAfterLoad()
    {
        AddCustomModSetting(maxBeaverMultiplier, nameof(MaxBeaverMultiplier));
        AddCustomModSetting(sleepSatisfactionMultiplier, nameof(SleepSatisfactionMultiplier));
        AddCustomModSetting(shelterSatisfactionMultiplier, nameof(ShelterSatisfactionMultiplier));
        AddCustomModSetting(moveEntranceFloor, nameof(MoveEntranceFloor));
        AddCustomModSetting(addOtherFaction, nameof(AddOtherFaction));

        UpdateValues();
    }

    void UpdateValues()
    {
        MaxBeaverMultiplier = maxBeaverMultiplier.Value;
        SleepSatisfactionMultiplier = sleepSatisfactionMultiplier.Value;
        ShelterSatisfactionMultiplier = shelterSatisfactionMultiplier.Value;
        MoveEntranceFloor = moveEntranceFloor.Value;
        AddOtherFaction = addOtherFaction.Value;
    }

    public void Unload()
    {
        UpdateValues();
    }
}
