namespace ModdableTimberborn.BuildingSettings.BuiltInSettings;

public record LeverSettingsModel(
    bool IsOn,
    bool IsSpringReturn,
    bool IsPinned
);

public class LeverSettings(ILoc t) : BuildingSettingsBase<Lever, LeverSettingsModel>(t)
{
    public override string DescribeModel(LeverSettingsModel model) => t.TYesNo(model.IsOn);

    protected override bool ApplyModel(LeverSettingsModel model, Lever target)
    {
        target.IsOn = model.IsOn;
        target.SetSpringReturn(model.IsSpringReturn);
        target.SetPinned(model.IsPinned);
        target.UpdateOutputState();

        return true;
    }

    protected override LeverSettingsModel GetModel(Lever target)
        => new(target.IsOn, target.IsSpringReturn, target.IsPinned);
}