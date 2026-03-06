namespace ModdableTimberborn.BuildingSettings.BuiltInSettings;

public record ClutchSettingsModel(ClutchMode Value) : ValueSettingModel<ClutchMode>(Value);

public class ClutchSettings(ILoc t) : BuildingSettingsBase<Clutch, ClutchSettingsModel>(t)
{
    public override string DescribeModel(ClutchSettingsModel model) => model.Value switch
    {
        ClutchMode.Engaged => t.T("Building.Clutch.Mode.Engaged"),
        ClutchMode.Disengaged => t.T("Building.Clutch.Mode.Disengaged"),
        ClutchMode.Automated => t.T("Automation.Mode.Automated"),
        _ => throw new ArgumentOutOfRangeException(nameof(model.Value)),
    };

    protected override bool ApplyModel(ClutchSettingsModel model, Clutch target)
    {
        target.Mode = model;
        target.ApplyState();
        return true;
    }

    protected override ClutchSettingsModel GetModel(Clutch target) => new(target.Mode);
}