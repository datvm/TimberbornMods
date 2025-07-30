namespace Bindito.Core;

public static class ModdableWeatherBindingExtensions
{

    public static T InstanceOrThrow<T>(this T? instance) where T : class
        => instance ?? throw new InvalidOperationException($"{typeof(T).Name} is not loaded yet!");

    public static T InstanceOrThrow<T>(this T? instance) where T : struct
        => instance ?? throw new InvalidOperationException($"{typeof(T).Name} is not loaded yet!");

    public static Configurator BindHazardousWeather<T>(this Configurator configurator)
        where T : class, IModdedHazardousWeather
    {
        configurator.MultiBindAndBindSingleton<IModdedHazardousWeather, T>();
        configurator.MultiBind<IModdedWeather>().ToExisting<T>();

        return configurator;
    }

    public static Configurator BindRainEffectWeather<T>(this Configurator configurator)
        where T : class, IRainEffectWeather
    {
        configurator.MultiBind<IRainEffectWeather>().ToExisting<T>();
        return configurator;
    }

    public static Configurator BindTemperateWeather<T>(this Configurator configurator)
        where T : class, IModdedTemperateWeather
    {
        configurator.MultiBindAndBindSingleton<IModdedTemperateWeather, T>();
        configurator.MultiBind<IModdedWeather>().ToExisting<T>();

        return configurator;
    }

    public static Configurator BindHazardousWeather<TWeather, TSettings>(this Configurator configurator, bool menuContext)
        where TWeather : DefaultModdedWeather<TSettings>, IModdedHazardousWeather
        where TSettings : DefaultWeatherSettings
    {
        configurator.BindSingleton<TSettings>();

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
        configurator.BindSingleton<TSettings>();
        if (!menuContext)
        {
            BindTemperateWeather<TWeather>(configurator);
        }
        return configurator;
    }

}

