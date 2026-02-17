namespace ModdableTimberborn.BuildingSettings.BuiltInSettings;

public readonly record struct TimedComponentActivatorSettingsModel(
    bool Enabled, 
    int CyclesUntilCountdownActivation,
    float DaysUntilActivation)
{

    [JsonIgnore]
    public TimedComponentActivator Component { get; } = new(null, null, null, null)
    {
        IsEnabled = Enabled,
        CyclesUntilCountdownActivation = CyclesUntilCountdownActivation,
        DaysUntilActivation = DaysUntilActivation
    };

}

public class TimedComponentActivatorSettings(ILoc t) : BuildingSettingsBase<TimedComponentActivator, TimedComponentActivatorSettingsModel>(t)
{
    public override string DescribeModel(TimedComponentActivatorSettingsModel model) => "";

    protected override bool ApplyModel(TimedComponentActivatorSettingsModel model, TimedComponentActivator target)
    {
        var tmp = new TimedComponentActivator(null, null, null, null)
        {
            IsEnabled = model.Enabled,
            CyclesUntilCountdownActivation = model.CyclesUntilCountdownActivation,
            DaysUntilActivation = model.DaysUntilActivation
        };

        target.DuplicateFrom(tmp);
        return true;
    }

    protected override TimedComponentActivatorSettingsModel GetModel(TimedComponentActivator duplicable)
        => new(duplicable.Enabled, duplicable.CyclesUntilCountdownActivation, duplicable.DaysUntilActivation);
}
