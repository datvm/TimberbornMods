namespace ModdableTimberborn.BuildingSettings.BuiltInSettings;

public record ChronometerSettingsModel(float StartTime, float EndTime, ChronometerMode Mode);

public class ChronometerSettings(ILoc t) : BuildingSettingsBase<Chronometer, ChronometerSettingsModel>(t)
{
    public override string DescribeModel(ChronometerSettingsModel model) => model.Mode switch
    {
        ChronometerMode.TimeRange => $"{t.THours(model.StartTime)} - {t.THours(model.EndTime)}",
        ChronometerMode.WorkingHours => t.T("Building.Chronometer.Mode.WorkingHours"),
        ChronometerMode.NonWorkingHours => t.T("Building.Chronometer.Mode.NonWorkingHours"),
        _ => throw new ArgumentOutOfRangeException(nameof(model.Mode)),
    };

    protected override bool ApplyModel(ChronometerSettingsModel model, Chronometer target)
    {
        target.StartTime = model.StartTime;
        target.EndTime = model.EndTime;
        target.Mode = model.Mode;
        target.UpdateOutputState();
        return true;
    }

    protected override ChronometerSettingsModel GetModel(Chronometer target) =>
        new(target.StartTime, target.EndTime, target.Mode);
}