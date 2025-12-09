namespace LateGamePower;
public class ModSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository)
{

    public override string ModId => nameof(LateGamePower);

    RangeIntModSetting? baseCost, maxMultiplier;
    
    public int BaseCost => baseCost?.Value ?? 10;
    public int MaxMultiplier => maxMultiplier?.Value ?? 10;

    public override void OnAfterLoad()
    {
        baseCost = new RangeIntModSetting(
            10, 0, 20,
            ModSettingDescriptor
                .CreateLocalized("LV.LGP.BaseCost")
                .SetLocalizedTooltip("LV.LGP.BaseCostDesc"));

        maxMultiplier = new RangeIntModSetting(
            10, 1, 30,
            ModSettingDescriptor
                .CreateLocalized("LV.LGP.MaxMul")
                .SetLocalizedTooltip("LV.LGP.MaxMulDesc"));

        AddCustomModSetting(baseCost, nameof(baseCost));
        AddCustomModSetting(maxMultiplier, nameof(maxMultiplier));
    }

}
