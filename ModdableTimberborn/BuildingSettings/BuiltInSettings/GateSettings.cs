namespace ModdableTimberborn.BuildingSettings.BuiltInSettings;

public class GateSettings(ILoc t) : BuildingSettingsBase<Gate, ValueSettingModel<GateOpeningMode>>(t)
{
    public override string DescribeModel(ValueSettingModel<GateOpeningMode> model) => t.T(model.Value switch
    {
        GateOpeningMode.Open => "Toggle.State.Closed",
        GateOpeningMode.Closed => "Toggle.State.Open",
        GateOpeningMode.Automated => "Automation.Mode.Automated",
        _ => throw new ArgumentOutOfRangeException(),
    });

    protected override bool ApplyModel(ValueSettingModel<GateOpeningMode> model, Gate target)
    {
        target._gateOpeningMode = model;
        target.UpdateState();
        return true;
    }

    protected override ValueSettingModel<GateOpeningMode> GetModel(Gate target)
        => new(target._gateOpeningMode);
}