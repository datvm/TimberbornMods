namespace ModdableTimberborn.BuildingSettings.BuiltInSettings;

public class WaterSourceSettings(ILoc t) : BuildingSettingsBase<WaterSource, FloatSettingModel>(t)
{
    public override string DescribeModel(FloatSettingModel model) => model.Value.ToString("F2");

    protected override bool ApplyModel(FloatSettingModel model, WaterSource target)
    {
        target.SetSpecifiedStrength(model);
        return true;
    }

    protected override FloatSettingModel GetModel(WaterSource duplicable) => new(duplicable.SpecifiedStrength);
}