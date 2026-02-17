namespace ModdableTimberborn.BuildingSettings.BuiltInSettings;

public class WaterSourceRegulatorSettings(ILoc t) : BuildingSettingsBase<WaterSourceRegulator, BoolSettingModel>(t)
{
    public override string DescribeModel(BoolSettingModel model) => model.T(t);

    protected override bool ApplyModel(BoolSettingModel model, WaterSourceRegulator target)
    {
        target.SetOpen(model);
        return true;
    }

    protected override BoolSettingModel GetModel(WaterSourceRegulator duplicable) => duplicable.IsOpen;
}