namespace ConfigurableHousing;

public class MSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository)
{

    public ModSetting<float> MaxBeaverMultiplier { get; } = new(
        1.5f,
        ModSettingDescriptor
            .CreateLocalized("LV.CH.MaxBeaverMul")
            .SetLocalizedTooltip("LV.CH.MaxBeaverMulDesc"));

    public ModSetting<int> MaxBeaverAdd { get; } = new(
        0,
        ModSettingDescriptor
            .CreateLocalized("LV.CH.MaxBeaverAdd")
            .SetLocalizedTooltip("LV.CH.MaxBeaverAddDesc"));

    public ModSetting<float> SleepSatisfactionMultiplier { get; } = new(
        1,
        ModSettingDescriptor
            .CreateLocalized("LV.CH.SleepMult")
            .SetLocalizedTooltip("LV.CH.SleepMultDesc"));

    public ModSetting<float> ShelterSatisfactionMultiplier { get; } = new(
        1,
        ModSettingDescriptor
            .CreateLocalized("LV.CH.ShelterMult")
            .SetLocalizedTooltip("LV.CH.ShelterMultDesc"));

    public ModSetting<bool> MoveEntranceFloor { get; } = new(
        false,
        ModSettingDescriptor
            .CreateLocalized("LV.CH.MoveEntranceFloor")
            .SetLocalizedTooltip("LV.CH.MoveEntranceFloorDesc"));

    public ModSetting<bool> AddOtherFaction { get; } = new(
        false,
        ModSettingDescriptor
            .CreateLocalized("LV.CH.AddOtherFaction")
            .SetLocalizedTooltip("LV.CH.AddOtherFactionDesc"));

    public ModSetting<bool> AddProcreation { get; } = new(
        false,
        ModSettingDescriptor
            .CreateLocalized("LV.CH.AddProcreation")
            .SetLocalizedTooltip("LV.CH.AddProcreationDesc"));

    public ModSetting<bool> RemoveProcreation { get; } = new(
        false,
        ModSettingDescriptor
            .CreateLocalized("LV.CH.RemoveProcreation")
            .SetLocalizedTooltip("LV.CH.RemoveProcreationDesc"));

    public override string ModId { get; } = nameof(ConfigurableHousing);

    public override void OnBeforeLoad()
    {
        base.OnBeforeLoad();

        AddProcreation.Descriptor.SetEnableCondition(() => AddOtherFaction.Value && !RemoveProcreation.Value);
        RemoveProcreation.Descriptor.SetEnableCondition(() => AddOtherFaction.Value && !AddProcreation.Value);
    }

}
