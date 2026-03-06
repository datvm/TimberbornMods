namespace ModdableTimberborn.BuildingSettings.BuiltInSettings;

public class DepthSensorSettings(ILoc t) : BuildingSettingsBase<DepthSensor, ComparisonSettingsModel>(t)
{
    public override string DescribeModel(ComparisonSettingsModel model) => model.ToString();

    protected override bool ApplyModel(ComparisonSettingsModel model, DepthSensor target)
    {
        target.InitializeSensorCoordinates();
        target._rawThreshold = target.GetCurrentFloor() + model.Threshold;
        target.Mode = model.Mode;
        target.UpdateOutputState();

        return true;
    }

    protected override ComparisonSettingsModel GetModel(DepthSensor target)
        => new(target.Mode, target.ThresholdFromFloor);

}