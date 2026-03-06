namespace ModdableTimberborn.BuildingSettings.BuiltInSettings;

public class ContaminationSensorSettings(ILoc t) : BuildingSettingsBase<ContaminationSensor, ComparisonSettingsModel>(t)
{
    public override string DescribeModel(ComparisonSettingsModel model) => model.ToString();

    protected override bool ApplyModel(ComparisonSettingsModel model, ContaminationSensor target)
    {
        target.Threshold = model.Threshold;
        target.Mode = model.Mode;
        target.UpdateOutputState();
        return true;
    }

    protected override ComparisonSettingsModel GetModel(ContaminationSensor target)
        => new(target.Mode, target.Threshold);
}