global using SkyblightWeather.Services;
global using SkyblightWeather.Weathers;

namespace SkyblightWeather;

[Context("MainMenu")]
public class ModMainMenuConfig : Configurator
{
    public override void Configure()
    {
        this
            .BindHazardousWeather<SkyblightWeatherType, SkyblightWeatherSettings>(true)
            .BindHazardousWeather<BadrainWeather, BadrainWeatherSettings>(true)
        ;
    }
}

[Context("Game")]
public class ModGameConfig : Configurator
{
    public override void Configure()
    {
        this
            .BindHazardousWeather<SkyblightWeatherType, SkyblightWeatherSettings>(false)
            .BindHazardousWeather<BadrainWeather, BadrainWeatherSettings>(false)

            .BindRainEffectWeather<BadrainWeather>()

            .BindSingleton<BlightApplier>()
        ;
    }
}
