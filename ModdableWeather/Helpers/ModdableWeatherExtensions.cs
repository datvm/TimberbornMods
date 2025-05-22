namespace Bindito.Core;

public static class ModdableWeatherExtensions
{

    public static Configurator BindHazardousWeather<T>(this Configurator configurator)
        where T : class, IModdedHazardousWeather
        => MultibindAndBind<IModdedHazardousWeather, T>(configurator);

    public static Configurator BindTemperateWeather<T>(this Configurator configurator)
        where T : class, IModdedTemperateWeather
        => MultibindAndBind<IModdedTemperateWeather, T>(configurator);


    static Configurator MultibindAndBind<TInterface, T>(Configurator configurator)
        where T : class, TInterface
        where TInterface : class
    {
        configurator.Bind<T>().AsSingleton();
        configurator.MultiBind<TInterface>().ToExisting<T>();

        return configurator;
    }

}
