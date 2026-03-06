namespace ModdableTimberborn.BuildingSettings.BuiltInSettings;

public class FlowSensorSettings(ILoc t) : BuildingSettingsBase<FlowSensor, ComparisonSettingsModel>(t)
{
    public override string DescribeModel(ComparisonSettingsModel model) => model.ToString();

    protected override bool ApplyModel(ComparisonSettingsModel model, FlowSensor target)
    {
        target.Mode = model.Mode;
        target.Threshold = model.Threshold;
        target.UpdateOutputState();
        return true;
    }

    protected override ComparisonSettingsModel GetModel(FlowSensor target)
        => new(target.Mode, target.Threshold);
}