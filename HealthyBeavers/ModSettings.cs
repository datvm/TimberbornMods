using ModSettings.Core;
using Timberborn.Modding;
using Timberborn.SettingsSystem;
using Timberborn.SingletonSystem;

namespace HealthyBeavers;

public class ModSettings
    : ModSettingsOwner, IUnloadableSingleton
{
    public static bool BrokenTeeth { get; private set; }
    public static bool BeeSting { get; private set; }
    public static bool BadwaterContamination { get; private set; }
    public static bool Injury { get; private set; }

    ModSetting<bool>? brokenTeeth, beeSting, badwaterContamination, injury;

    public ModSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository) : base(settings, modSettingsOwnerRegistry, modRepository)
    {
    }

    protected override string ModId => nameof(HealthyBeavers);

    protected override void OnAfterLoad()
    {
        UnityEngine.Debug.Log($"[HealthyBeavers] Loaded");

        brokenTeeth = new ModSetting<bool>(
            false,
            ModSettingDescriptor
                .Create("Broken Teeth")
                .SetTooltip("Disable Broken Teeth"));

        beeSting = new ModSetting<bool>(
            true,
            ModSettingDescriptor
                .Create("Bee Sting")
                .SetTooltip("Disable Bee Sting"));

        badwaterContamination = new ModSetting<bool>(
            false,
            ModSettingDescriptor
                .Create("Badwater Contamination")
                .SetTooltip("Disable Badwater Contamination"));

        injury = new ModSetting<bool>(
            false,
            ModSettingDescriptor
                .Create("Injury")
                .SetTooltip("Disable Injury"));

        AddCustomModSetting(brokenTeeth, nameof(BrokenTeeth));
        AddCustomModSetting(beeSting, nameof(BeeSting));
        AddCustomModSetting(badwaterContamination, nameof(BadwaterContamination));
        AddCustomModSetting(injury, nameof(Injury));

        UpdateValues();
    }

    void UpdateValues()
    {
        UnityEngine.Debug.Log($"[HealthyBeavers] Updated values");

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
