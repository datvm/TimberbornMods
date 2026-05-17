namespace MoreHttpApi.Shared.BuildingSettings;

public record WeatherStationSettingsModel(
    HttpWeatherStationMode Mode,
    bool EarlyActivationEnabled,
    int EarlyActivationHours
);

public enum HttpWeatherStationMode
{
    Temperate,
    Drought,
    Badtide
}
