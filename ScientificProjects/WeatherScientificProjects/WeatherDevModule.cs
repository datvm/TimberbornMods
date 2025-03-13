global using Timberborn.Debugging;
global using Timberborn.WeatherSystem;

namespace WeatherScientificProjects;

public class WeatherDevModule(DialogBoxShower shower, WeatherService weather) : IDevModule
{
    public DevModuleDefinition GetDefinition()
    {
        return new DevModuleDefinition.Builder()
            .AddMethod(new("Weather: Show badweather", null, ShowWeather))
            .Build();
    }

    void ShowWeather()
    {
        var today = weather._gameCycleService.CycleDay;
        var temperate = weather.TemperateWeatherDuration;
        var hazardLen = weather.HazardousWeatherDuration;

        var hazardName = weather._hazardousWeatherService.CurrentCycleHazardousWeather.Id;
        var approachingDay = WeatherUpgradeProcessor.WarningDays;

        var msg = $"""
            Current day: {today}
            Temperate weather duration: {temperate} ({(temperate - today) + 1} days left)
            Hazardous weather: {hazardName} for {hazardLen} days.
            Approaching notification: {approachingDay} days.
            """;

        shower.Create()
            .SetMessage(msg)
            .Show();
    }

}
