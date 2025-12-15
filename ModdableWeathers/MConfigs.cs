
namespace ModdableWeathers;

public class ModdableWeatherConfig : BaseModdableTimberbornConfigurationWithHarmony
{
    public override ConfigurationContext AvailableContexts { get; } = ConfigurationContext.Game;

    public override void Configure(Configurator configurator, ConfigurationContext context)
    {
        configurator
            // History
            .BindSingleton<WeatherHistoryRegistry>()
            .BindSingleton<WeatherHistoryService>()

            // Weather Services
            .BindSingleton<ModdableWeatherSpecService>()
            .BindSingleton<ModdableWeatherRegistry>()
            .BindSingleton<ModdableWeatherSettingsService>()

            // Game Built-in Weathers
            .BindWeather<GameTemperateWeather>()
            .BindWeather<GameDroughtWeather>()
            .BindWeather<GameBadtideWeather>()

            // Mod built-in Weathers
            .BindWeather<RainModdableWeather, RainModdableWeatherSettings>()
            .BindRainEffectWeather<RainModdableWeather>()

            // Special weathers, only bind, don't multibind
            .BindSingleton<EmptyBenignWeather>()
            .BindSingleton<EmptyHazardousWeather>()
        ;
    }

}
