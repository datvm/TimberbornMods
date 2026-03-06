namespace ModdableTimberborn.BuildingSettings.BuiltInSettings;

public record WaterSourceRegulatorSettingsModel(WaterSourceRegulator.RegulatorState State);

public class WaterSourceRegulatorSettings(ILoc t) : BuildingSettingsBase<WaterSourceRegulator, WaterSourceRegulatorSettingsModel>(t)
{
    public override string DescribeModel(WaterSourceRegulatorSettingsModel model) => t.T("LV.MT.State_" + model.State.ToString());

    protected override bool ApplyModel(WaterSourceRegulatorSettingsModel model, WaterSourceRegulator target)
    {
        target.SetRegulatorState(model.State);
        return true;
    }

    protected override WaterSourceRegulatorSettingsModel GetModel(WaterSourceRegulator duplicable) => new(duplicable._regulatorState);
}