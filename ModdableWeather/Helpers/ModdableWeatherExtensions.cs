namespace Bindito.Core;

public static class ModdableWeatherExtensions
{

    public static T InstanceOrThrow<T>(this T? instance) where T : class 
        => instance ?? throw new InvalidOperationException($"{typeof(T).Name} is not loaded yet!");

    public static Configurator BindHazardousWeather<T>(this Configurator configurator)
        where T : class, IModdedHazardousWeather
        => MultibindAndBind<IModdedHazardousWeather, T>(configurator);

    public static Configurator BindTemperateWeather<T>(this Configurator configurator)
        where T : class, IModdedTemperateWeather
        => MultibindAndBind<IModdedTemperateWeather, T>(configurator);

    public static Configurator BindHazardousWeather<TWeather, TSettings>(this Configurator configurator, bool menuContext)
        where TWeather : DefaultModdedWeather<TSettings>, IModdedHazardousWeather
        where TSettings : DefaultWeatherSettings
    {
        configurator.Bind<TSettings>().AsSingleton();

        if (!menuContext)
        {
            BindHazardousWeather<TWeather>(configurator);
        }

        return configurator;
    }

    public static Configurator BindTemperateWeather<TWeather, TSettings>(this Configurator configurator, bool menuContext)
        where TWeather : DefaultModdedWeather<TSettings>, IModdedTemperateWeather
        where TSettings : DefaultWeatherSettings
    {
        configurator.Bind<TSettings>().AsSingleton();
        if (!menuContext)
        {
            BindTemperateWeather<TWeather>(configurator);
        }
        return configurator;
    }

    public static Configurator RemoveBinding<T>(this Configurator configurator)
    {
        var registry = GetRegistry(configurator);

        registry._boundBindingBuilders.Remove(typeof(T));
        return configurator;
    }

    public static Configurator ReplaceBinding<T, TRep>(this Configurator configurator)
        where TRep : class, T
        where T : class
    {
        configurator.RemoveBinding<T>();
        configurator.Bind<T>().To<TRep>().AsSingleton();

        return configurator;
    }

    public static Configurator RemoveMultibinding<T>(this Configurator configurator)
        where T : class
    {
        var registry = GetRegistry(configurator);
        registry._boundMultiBindingBuilders.Remove(typeof(T));
        return configurator;
    }

    static BindingBuilderRegistry GetRegistry(Configurator configurator)
    {
        var def = (ContainerDefinition)configurator._containerDefinition;
        var registry = (BindingBuilderRegistry)def._bindingBuilderRegistry;

        return registry;
    }

    static Configurator MultibindAndBind<TInterface, T>(Configurator configurator)
        where T : class, TInterface
        where TInterface : class
    {
        configurator.Bind<T>().AsSingleton();
        configurator.MultiBind<TInterface>().ToExisting<T>();

        return configurator;
    }

}
