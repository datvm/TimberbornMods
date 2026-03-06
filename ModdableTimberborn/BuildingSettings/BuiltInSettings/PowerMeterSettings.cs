namespace ModdableTimberborn.BuildingSettings.BuiltInSettings;

public record PowerMeterSettingsModel(
    PowerMeterMode Mode,
    NumericComparisonMode ComparisonMode,
    int IntThreshold,
    float PercentThreshold
);

public class PowerMeterSettings(ILoc t) : BuildingSettingsBase<PowerMeter, PowerMeterSettingsModel>(t)
{
    public override string DescribeModel(PowerMeterSettingsModel model)
        => $"{t.T("Building.PowerMeter.Mode." + model.Mode)} {model.ComparisonMode.ToChar()} "
        + (IsPercentThreshold(model.Mode) ? model.PercentThreshold.ToString("0%") : model.IntThreshold.ToString());

    static bool IsPercentThreshold(PowerMeterMode mode) => mode == PowerMeterMode.BatteryChargeLevel;

    protected override bool ApplyModel(PowerMeterSettingsModel model, PowerMeter target)
    {
        target.Mode = model.Mode;
        target.ComparisonMode = model.ComparisonMode;
        target.IntThreshold = model.IntThreshold;
        target.PercentThreshold = model.PercentThreshold;
        target.UpdateState();
        
        return true;
    }

    protected override PowerMeterSettingsModel GetModel(PowerMeter target) 
        => new(target.Mode, target.ComparisonMode, target.IntThreshold, target.PercentThreshold);
}