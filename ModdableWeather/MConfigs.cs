namespace ModdableWeather;

[Context("MainMenu")]
public class ModMenuConfig : Configurator
{

    public override void Configure()
    {
        Bind<ModdableWeatherSpecService>().AsSingleton();

        this.BindTemperateWeather<GameTemperateWeather, GameTemperateWeatherSettings>(true);

        this.BindHazardousWeather<GameDroughtWeather, GameDroughtWeatherSettings>(true);
        this.BindHazardousWeather<GameBadtideWeather, GameBadtideWeatherSettings>(true);
    }

}

[Context("Game")]
public class ModGameConfig : Configurator
{

    public override void Configure()
    {
        // Weathers
        this.BindTemperateWeather<GameTemperateWeather, GameTemperateWeatherSettings>(false);

        this.BindHazardousWeather<GameDroughtWeather, GameDroughtWeatherSettings>(false);
        this.BindHazardousWeather<GameBadtideWeather, GameBadtideWeatherSettings>(false);

        // Cycle
        this.RemoveMultibinding<ICycleDuration>();
        Bind<ModdableWeatherCycleService>().AsSingleton();
        MultiBind<ICycleDuration>().ToExisting<ModdableWeatherCycleService>();

        // Old Services
        this.ReplaceBinding<WeatherService, ModdableWeatherService>();
        this.ReplaceBinding<HazardousWeatherService, ModdableHazardousWeatherService>();
        this.ReplaceBinding<HazardousWeatherApproachingTimer, ModdableHazardousWeatherApproachingTimer>();

        // New Services
        Bind<ModdableWeatherSpecService>().AsSingleton();
        Bind<ModdableWeatherRegistry>().AsSingleton();
        Bind<ModdableWeatherHistoryProvider>().AsSingleton();
        Bind<ModdableWeatherService>().AsSingleton();
        Bind<ModdableWeatherGenerator>().AsSingleton();

        // Bind but do NOT multibind this special weather for record keeping
        Bind<NoneHazardousWeather>().AsSingleton();
    }

}
