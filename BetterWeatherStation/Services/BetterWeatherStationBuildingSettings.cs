namespace BetterWeatherStation.Services;

public record BetterWeatherStationBuildingSettingsModel(
    string[] WeatherIds
);

[MultiBind(typeof(IBuildingSettings))]
public class BetterWeatherStationBuildingSettings(
    ILoc t,
    WeatherStationInfoService service
) : BuildingSettingsBase<BetterWeatherStationComponent, BetterWeatherStationBuildingSettingsModel>(t)
{
    public override string DescribeModel(BetterWeatherStationBuildingSettingsModel model)
    {
        if (model.WeatherIds.Length == 0) { return t.TNone(); }
        return string.Join(", ", model.WeatherIds.Select(id => service.GetOrDefault(id).Name));
    }

    protected override bool ApplyModel(BetterWeatherStationBuildingSettingsModel model, BetterWeatherStationComponent target)
    {
        target.SetWeathers(model.WeatherIds);
        return true;
    }

    protected override BetterWeatherStationBuildingSettingsModel GetModel(BetterWeatherStationComponent duplicable)
        => new([.. duplicable.WeatherIds]);
}
