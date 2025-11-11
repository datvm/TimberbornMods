namespace ConfigurablePumps;

public class MSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository)
   : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository), IUnloadableSingleton
{
    public const float DefaultMechPumpAmount = .25f;

    public override string ModId { get; } = nameof(ConfigurablePumps);

    readonly ModSetting<bool> allFixedDepth = new(false, ModSettingDescriptor
        .CreateLocalized("LV.SPE.AllFixedDepth")
        .SetLocalizedTooltip("LV.SPE.AllFixedDepthDesc"));
    readonly ModSetting<int> fixedDepth = new(3, ModSettingDescriptor
        .CreateLocalized("LV.SPE.FixedDepth")
        .SetLocalizedTooltip("LV.SPE.FixedDepthDesc"));
    readonly ModSetting<bool> allMultiplier = new(true, ModSettingDescriptor
        .CreateLocalized("LV.SPE.AllMultiplier")
        .SetLocalizedTooltip("LV.SPE.AllMultiplierDesc"));
    readonly ModSetting<float> multiplier = new(2f, ModSettingDescriptor
        .CreateLocalized("LV.SPE.Multiplier")
        .SetLocalizedTooltip("LV.SPE.MultiplierDesc"));
    readonly ModSetting<float> mechPumpWater = new(DefaultMechPumpAmount, ModSettingDescriptor
        .CreateLocalized("LV.SPE.MechPumpWater")
        .SetLocalizedTooltip("LV.SPE.MechPumpWaterDesc"));
    readonly ModSetting<float> waterProdTimeMultiplier = new(1f, ModSettingDescriptor
        .CreateLocalized("LV.SPE.WaterProdTimeMultiplier")
        .SetLocalizedTooltip("LV.SPE.WaterProdTimeMultiplierDesc"));
    readonly ModSetting<float> mechPumpPowerMultiplier = new(1f, ModSettingDescriptor
        .CreateLocalized("LV.SPE.MechPumpPowerMultiplier")
        .SetLocalizedTooltip("LV.SPE.MechPumpPowerDesc"));
    readonly ModSetting<float> waterConversation = new(0.2f, ModSettingDescriptor
        .CreateLocalized("LV.SPE.WaterConversation")
        .SetLocalizedTooltip("LV.SPE.WaterConversationDesc"));

    public static bool AllFixedDepth { get; private set; } = false;
    public static int FixedDepth { get; private set; } = 0;
    public static bool AllMultiplier { get; private set; } = false;
    public static float Multiplier { get; private set; } = 1f;
    public static float MechPumpWater { get; private set; } = DefaultMechPumpAmount;
    public static float WaterProdTimeMultiplier { get; private set; } = 1f;
    public static float MechPumpPowerMultiplier { get; private set; } = 1f;
    public static float WaterConversation { get; private set; } = 0.2f;

    public override void OnAfterLoad()
    {
        allFixedDepth.Descriptor.SetEnableCondition(() => !allMultiplier.Value);
        allMultiplier.Descriptor.SetEnableCondition(() => !allFixedDepth.Value);

        fixedDepth.Descriptor.SetEnableCondition(() => allFixedDepth.Value);
        multiplier.Descriptor.SetEnableCondition(() => allMultiplier.Value);

        AddCustomModSetting(allMultiplier, nameof(allMultiplier));
        AddCustomModSetting(multiplier, nameof(multiplier));
        AddCustomModSetting(allFixedDepth, nameof(allFixedDepth));
        AddCustomModSetting(fixedDepth, nameof(fixedDepth));

        AddCustomModSetting(waterProdTimeMultiplier, nameof(waterProdTimeMultiplier));
        AddCustomModSetting(waterConversation, nameof(waterConversation));

        AddCustomModSetting(mechPumpWater, nameof(mechPumpWater));
        AddCustomModSetting(mechPumpPowerMultiplier, nameof(mechPumpPowerMultiplier));

        UpdateValues();
    }

    void UpdateValues()
    {
        AllFixedDepth = allFixedDepth.Value;
        FixedDepth = fixedDepth.Value;
        AllMultiplier = allMultiplier.Value;
        Multiplier = multiplier.Value;
        MechPumpWater = mechPumpWater.Value;
        WaterProdTimeMultiplier = waterProdTimeMultiplier.Value;
        MechPumpPowerMultiplier = mechPumpPowerMultiplier.Value;
        WaterConversation = waterConversation.Value;

        if (FixedDepth < 1)
        {
            FixedDepth = fixedDepth.Value = 1;
        }

        if (Multiplier <= 0)
        {
            Multiplier = multiplier.Value = 1.0f;
        }

        if (MechPumpWater <= 0)
        {
            MechPumpWater = mechPumpWater.Value = 0.25f;
        }

        if (WaterProdTimeMultiplier <= 0)
        {
            WaterProdTimeMultiplier = waterProdTimeMultiplier.Value = 1.0f;
        }

        if (MechPumpPowerMultiplier <= 0)
        {
            MechPumpPowerMultiplier = mechPumpPowerMultiplier.Value = 1.0f;
        }

        if (WaterConversation <= 0)
        {
            WaterConversation = waterConversation.Value = 0.2f;
        }

        OnWaterConversionChanged();
    }

    public void Unload()
    {
        UpdateValues();
    }

    // While this is kept in V1 for reference purpose, the value will not be used and will be patched instead
    static readonly FieldInfo WaterAmountConversionField = typeof(WaterGoodToWaterAmountConverter).Field("WaterAmountConversion");
    static readonly FieldInfo BadWaterAmountConversionField = typeof(WaterContaminationGoodToWaterContaminationAmountConverter).Field("WaterContaminationAmountConversion");
    static void OnWaterConversionChanged()
    {
        WaterAmountConversionField.SetValue(null, WaterConversation);
        BadWaterAmountConversionField.SetValue(null, WaterConversation);
    }

}
