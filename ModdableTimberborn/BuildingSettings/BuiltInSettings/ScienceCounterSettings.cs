namespace ModdableTimberborn.BuildingSettings.BuiltInSettings;

public class ScienceCounterSettings(ILoc t) : BuildingSettingsBase<ScienceCounter, ComparisonSettingsModel>(t)
{
    public override string DescribeModel(ComparisonSettingsModel model) => model.ToString();

    protected override bool ApplyModel(ComparisonSettingsModel model, ScienceCounter target)
    {
        target.Mode = model.Mode;
        target.Threshold = (int)model.Threshold;
        target.UpdateOutputState();
        return true;
    }

    protected override ComparisonSettingsModel GetModel(ScienceCounter target) =>
        new(target.Mode, target.Threshold);
}