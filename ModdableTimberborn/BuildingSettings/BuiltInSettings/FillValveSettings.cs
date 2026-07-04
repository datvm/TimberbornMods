namespace ModdableTimberborn.BuildingSettings.BuiltInSettings;

public record FillValveSettingsModel(
    bool IsSynchronized,
    float TargetHeight,
    bool TargetHeightEnabled,
    float AutomationTargetHeight,
    bool AutomationTargetHeightEnabled
);

public class FillValveSettings(ILoc t) : BuildingSettingsBase<FillValve, FillValveSettingsModel>(t)
{
    public override string DescribeModel(FillValveSettingsModel model) => t.T("LV.MT.BldSet.FillValve.Desc",
        model.TargetHeight, t.TYesNo(model.IsSynchronized));

    protected override bool ApplyModel(FillValveSettingsModel model, FillValve target)
    {
        // OutputCoordinates is deliberately excluded: it is positional state, not a shared
        // setting (mirrors ThrottlingValveSettings, which likewise copies no coordinates).
        target.IsSynchronized = model.IsSynchronized;
        target.SetTargetHeight(model.TargetHeight);
        target.SetTargetHeightEnabled(model.TargetHeightEnabled);
        target.SetAutomationTargetHeight(model.AutomationTargetHeight);
        target.SetAutomationTargetHeightEnabled(model.AutomationTargetHeightEnabled);

        target.SynchronizeNeighbors();

        return true;
    }

    protected override FillValveSettingsModel GetModel(FillValve target)
        => new(target.IsSynchronized, target.TargetHeight, target.TargetHeightEnabled, target.AutomationTargetHeight, target.AutomationTargetHeightEnabled);
}
