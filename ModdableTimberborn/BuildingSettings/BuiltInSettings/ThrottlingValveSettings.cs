namespace ModdableTimberborn.BuildingSettings.BuiltInSettings;

public record ThrottlingValveSettingsModel(
    bool IsSynchronized,
    float OutflowLimit,
    bool OutflowLimitEnabled,
    float AutomationOutflowLimit,
    bool AutomationOutflowLimitEnabled,
    float ReactionSpeed
);

public class ThrottlingValveSettings(ILoc t) : BuildingSettingsBase<ThrottlingValve, ThrottlingValveSettingsModel>(t)
{
    public override string DescribeModel(ThrottlingValveSettingsModel model) => "";

    protected override bool ApplyModel(ThrottlingValveSettingsModel model, ThrottlingValve target)
    {
        target.IsSynchronized = model.IsSynchronized;
        target.SetOutflowLimit(model.OutflowLimit);
        target.SetOutflowLimitEnabled(model.OutflowLimitEnabled);
        target.SetAutomationOutflowLimit(model.AutomationOutflowLimit);
        target.SetAutomationOutflowLimitEnabled(model.AutomationOutflowLimitEnabled);
        target.SetReactionSpeed(model.ReactionSpeed);

        target.SynchronizeNeighbors();

        return true;
    }

    protected override ThrottlingValveSettingsModel GetModel(ThrottlingValve target)
        => new(target.IsSynchronized, target.OutflowLimit, target.OutflowLimitEnabled, target.AutomationOutflowLimit, target.AutomationOutflowLimitEnabled, target.ReactionSpeed);
}