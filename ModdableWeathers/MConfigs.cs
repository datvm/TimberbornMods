namespace ModdableWeathers;

public class ModdableWeatherConfig : BaseModdableTimberbornConfigurationWithHarmony
{
    public override ConfigurationContext AvailableContexts { get; } = ConfigurationContext.Game;

    public override void Configure(Configurator configurator, ConfigurationContext context)
    {
        configurator
            // History
            .BindSingleton<WeatherCycleStageDefinitionService>()
            .BindSingleton<WeatherHistoryRegistry>()
            .BindSingleton<WeatherHistoryService>()
            .BindSingleton<WeatherGenerator>()

            // Weather Services
            .BindSingleton<ModdableWeatherSpecService>()
            .BindSingleton<ModdableWeatherRegistry>()
            .BindSingleton<ModdableWeatherSettingsService>()

            // Weather Modifiers
            .BindSingleton<ModdableWeatherModifierSpecService>()
            .BindSingleton<ModdableWeatherModifierRegistry>()
            .BindSingleton<ModdableWeatherModifierSettingsService>()

            // Game Built-in Weathers
            .BindWeather<GameTemperateWeather, GameTemperateWeatherSettings>()
            .BindWeather<GameDroughtWeather, GameDroughtWeatherSettings>()
            .BindWeather<GameBadtideWeather, GameBadtideWeatherSettings>()

            // Mod built-in Weathers
            .BindWeather<RainModdableWeather, RainModdableWeatherSettings>()
            .BindWeather<MonsoonModdableWeather, MoonsoonWeatherSettings>()
            .BindWeather<SurprisinglyRefreshingModdableWeather, SurprisinglyRefreshingWeatherSettings>()

            // Mod built-in Modifiers
            .BindWeatherModifier<DroughtModifier, DroughtModifierSettings>()
            .BindWeatherModifier<BadtideModifier, BadtideModifierSettings>()
            .BindWeatherModifier<RainModifier, RainModifierSettings>().BindRainEffect<RainModifier>()
            .BindWeatherModifier<MonsoonModifier, MonsoonModifierSettings>()
            .BindWeatherModifier<RefreshingModifier, RefreshingModifierSettings>()

            // Special weathers, only bind, don't multibind
            .BindSingleton<EmptyBenignWeather>()
            .BindSingleton<EmptyHazardousWeather>()

            // Settings Dialog
            .BindTransient<WeatherSettingsDialog>()
            .BindSingleton<WeatherSettingsDialogShower>()

            // Rain
            .BindSingleton<RainEffectPlayer>()
        ;
    }

}
