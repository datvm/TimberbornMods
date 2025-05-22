namespace ModdableWeather;

[Context("MainMenu")]
public class ModMenuConfig : Configurator
{

    public override void Configure()
    {

    }

}

[Context("Game")]
public class ModGameConfig : Configurator
{

    public override void Configure()
    {
        Bind<ModdableWeatherRegistry>().AsSingleton();
        Bind<ModdableWeatherHistoryProvider>().AsSingleton();
        Bind<ModdableWeatherService>().AsSingleton();

        // Bind but do NOT multibind this
        Bind<NoneHazardousWeather>().AsSingleton();
    }

}
