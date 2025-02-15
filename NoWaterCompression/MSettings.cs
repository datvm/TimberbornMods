namespace NoWaterCompression;

public class MSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository,
    WaterSimulator waterSim) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository)
{

    public override string ModId => nameof(NoWaterCompression);
    public override ModSettingsContext ChangeableOn => ModSettingsContext.All;

    readonly RangeIntModSetting multiplier = new(15, 1, 100, ModSettingDescriptor.Create("Multiplier"));

    public override void OnAfterLoad()
    {
        AddCustomModSetting(multiplier, nameof(multiplier));

        multiplier.ValueChanged += (_, _) => ChangeValue();
        ChangeValue();
    }

    void ChangeValue()
    {
        //waterSim._simulationCount = multiplier.Value;
    }

}
