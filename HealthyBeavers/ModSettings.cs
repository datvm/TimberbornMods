namespace HealthyBeavers;

public class ModSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository)
    : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository), IUnloadableSingleton
{
    public static bool BrokenTeeth { get; private set; }
    public static bool BeeSting { get; private set; }
    public static bool BadwaterContamination { get; private set; }
    public static bool Injury { get; private set; }

    ModSetting<bool>? brokenTeeth, beeSting, badwaterContamination, injury;

    public override string ModId => nameof(HealthyBeavers);
    public override ModSettingsContext ChangeableOn => ModSettingsContext.All;

    public override void OnAfterLoad()
    {
        brokenTeeth = new ModSetting<bool>(
            false,
            ModSettingDescriptor
                .CreateLocalized("HB.BrokenTeeth")
                .SetLocalizedTooltip("HB.BrokenTeethDesc"));

        beeSting = new ModSetting<bool>(
            true,
            ModSettingDescriptor
                .CreateLocalized("HB.BeeSting")
                .SetLocalizedTooltip("HB.BeeStingDesc"));

        badwaterContamination = new ModSetting<bool>(
            false,
            ModSettingDescriptor
                .CreateLocalized("HB.BadwaterContamination")
                .SetLocalizedTooltip("HB.BadwaterContaminationDesc"));

        injury = new ModSetting<bool>(
            false,
            ModSettingDescriptor
                .CreateLocalized("HB.Injury")
                .SetLocalizedTooltip("HB.InjuryDesc"));

        AddCustomModSetting(brokenTeeth, nameof(BrokenTeeth));
        AddCustomModSetting(beeSting, nameof(BeeSting));
        AddCustomModSetting(badwaterContamination, nameof(BadwaterContamination));
        AddCustomModSetting(injury, nameof(Injury));

        brokenTeeth.ValueChanged += (_, _) => UpdateValues();
        beeSting.ValueChanged += (_, _) => UpdateValues();
        badwaterContamination.ValueChanged += (_, _) => UpdateValues();
        injury.ValueChanged += (_, _) => UpdateValues();

        UpdateValues();
    }

    void UpdateValues()
    {
        BrokenTeeth = brokenTeeth?.Value ?? false;
        BeeSting = beeSting?.Value ?? false;
        BadwaterContamination = badwaterContamination?.Value ?? false;
        Injury = injury?.Value ?? false;
    }

    public void Unload()
    {
        UpdateValues();
    }
}
