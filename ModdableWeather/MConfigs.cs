
namespace ModdableWeather;

[Context("MainMenu")]
public class ModMenuConfig : Configurator
{

    public override void Configure()
    {
        Bind<ModdableWeatherSpecService>().AsSingleton();

        this.BindTemperateWeather<GameTemperateWeather, GameTemperateWeatherSettings>(true)
            .BindHazardousWeather<GameDroughtWeather, GameDroughtWeatherSettings>(true)
            .BindHazardousWeather<GameBadtideWeather, GameBadtideWeatherSettings>(true);
    }

}

[Context("Game")]
public class ModGameConfig : Configurator
{

    public override void Configure()
    {
        // Weathers
        this
            .BindTemperateWeather<GameTemperateWeather, GameTemperateWeatherSettings>(false)
            .BindHazardousWeather<GameDroughtWeather, GameDroughtWeatherSettings>(false)
            .BindHazardousWeather<GameBadtideWeather, GameBadtideWeatherSettings>(false);

        // Replace/remove Services
        this.MassRebind()
            .Replace<WeatherService, ModdableWeatherService>()
            .Replace<HazardousWeatherService, ModdableHazardousWeatherService>()
            .Replace<HazardousWeatherApproachingTimer, ModdableHazardousWeatherApproachingTimer>()
            .Replace<HazardousWeatherNotificationPanel, ModdableHazardousWeatherNotificationPanel>()
            .Replace<DatePanel, ModdableDatePanel>()
            .Replace<WeatherPanel, ModdableWeatherPanel>()
            .Replace<Sun, ModdableSun>()
            .Replace<DayStageCycle, ModdableDayStageCycle>()

            .Remove<HazardousWeatherSoundPlayer>()
        .Bind();
            
        this.RemoveMultibinding<ICycleDuration>();
        this.MultiBindAndBindSingleton<ICycleDuration, ModdableWeatherCycleService>();

        this.BindSingleton<ModdableHazardousWeatherSoundPlayer>();

        // New Services
        this
            .BindSingleton<ModdableWeatherSpecService>()
            .BindSingleton<ModdableWeatherRegistry>()
            .BindSingleton<ModdableWeatherHistoryProvider>()
            .BindSingleton<ModdableWeatherGenerator>()

            // Bind but do NOT multibind this special weather for record keeping
            .BindSingleton<NoneHazardousWeather>();
    }

}
