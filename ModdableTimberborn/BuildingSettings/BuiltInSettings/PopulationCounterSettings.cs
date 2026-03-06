namespace ModdableTimberborn.BuildingSettings.BuiltInSettings;

public record PopulationCounterSettingsModel(
    PopulationCounterMode CounterMode,
    NumericComparisonMode Mode,
    float Threshold,
    bool CountBeavers,
    bool CountBots,
    bool GlobalMode
) : ComparisonSettingsModel(Mode, Threshold);

public class PopulationCounterSettings(ILoc t) : BuildingSettingsBase<PopulationCounter, PopulationCounterSettingsModel>(t)
{
    public override string DescribeModel(PopulationCounterSettingsModel model)
        => $"{t.T("Building.PopulationCounter.Mode." + model.CounterMode)} {model.Mode.ToChar()} {model.Threshold:0}";

    protected override bool ApplyModel(PopulationCounterSettingsModel model, PopulationCounter target)
    {
        target.Mode = model.CounterMode;
        target.ComparisonMode = model.Mode;
        target.SetGlobalModeInternal(model.GlobalMode, false);
        target.CountBeavers = model.CountBeavers;
        target.CountBots = model.CountBots;
        target.Threshold = (int)model.Threshold;
        target.Sample();

        return true;
    }

    protected override PopulationCounterSettingsModel GetModel(PopulationCounter target)
        => new(target.Mode, target.ComparisonMode, target.Threshold, target.CountBeavers, target.CountBots, target.GlobalMode);
}