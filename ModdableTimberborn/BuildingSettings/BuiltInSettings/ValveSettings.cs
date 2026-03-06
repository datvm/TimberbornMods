namespace ModdableTimberborn.BuildingSettings.BuiltInSettings;

public record ValveSettingsModel(
    bool IsSynchronized,
    float OutflowLimit,
    bool OutflowLimitEnabled,
    float AutomationOutflowLimit,
    bool AutomationOutflowLimitEnabled,
    float ReactionSpeed
);

public class ValveSettings(ILoc t) : BuildingSettingsBase<Valve, ValveSettingsModel>(t)
{
    public override string DescribeModel(ValveSettingsModel model) => "";

    protected override bool ApplyModel(ValveSettingsModel model, Valve target)
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

    protected override ValveSettingsModel GetModel(Valve target)
        => new(target.IsSynchronized, target.OutflowLimit, target.OutflowLimitEnabled, target.AutomationOutflowLimit, target.AutomationOutflowLimitEnabled, target.ReactionSpeed);
}