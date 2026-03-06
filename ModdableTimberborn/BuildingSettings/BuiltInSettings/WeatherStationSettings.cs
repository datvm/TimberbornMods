namespace ModdableTimberborn.BuildingSettings.BuiltInSettings;

public record WeatherStationSettingsModel(
    WeatherStationMode Mode,
    bool EarlyActivationEnabled,
    int EarlyActivationHours
);

public class WeatherStationSettings(ILoc t) : BuildingSettingsBase<WeatherStation, WeatherStationSettingsModel>(t)
{
    public override string DescribeModel(WeatherStationSettingsModel model) =>
        t.T("Weather." + model.Mode);

    protected override bool ApplyModel(WeatherStationSettingsModel model, WeatherStation target)
    {
        target.Mode = model.Mode;
        target.EarlyActivationEnabled = model.EarlyActivationEnabled;
        target.EarlyActivationHours = model.EarlyActivationHours;
        target.UpdateOutputState();

        return true;
    }

    protected override WeatherStationSettingsModel GetModel(WeatherStation target)
        => new(target.Mode, target.EarlyActivationEnabled, target.EarlyActivationHours);
}