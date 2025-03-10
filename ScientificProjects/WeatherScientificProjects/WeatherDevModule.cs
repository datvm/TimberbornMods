global using Timberborn.CoreUI;
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

        var msg = $"""
            Current day: {today}
            Temperate weather duration: {temperate} ({temperate - today} days left)
            Hazardous weather: {hazardName} for {hazardLen} days
            """;

        shower.Create()
            .SetMessage(msg)
            .Show();
    }

}
