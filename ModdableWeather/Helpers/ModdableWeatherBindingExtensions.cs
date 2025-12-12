namespace Bindito.Core;

public static class ModdableWeatherBindingExtensions
{

    public static T InstanceOrThrow<T>(this T? instance) where T : class
        => instance ?? throw new InvalidOperationException($"{typeof(T).Name} is not loaded yet!");

    public static T InstanceOrThrow<T>(this T? instance) where T : struct
        => instance ?? throw new InvalidOperationException($"{typeof(T).Name} is not loaded yet!");

    public static Configurator BindWeather<T>(this Configurator configurator)
        where T : ModdableWeatherBase
    {
        var isBenign = typeof(IModdableBenignWeather).IsAssignableFrom(typeof(T));
        var isHazardous = typeof(IModdableHazardousWeather).IsAssignableFrom(typeof(T));

        if (!isBenign && !isHazardous)
        {
            throw new InvalidOperationException($"{typeof(T).Name} must implement either {nameof(IModdableBenignWeather)} or {nameof(IModdableHazardousWeather)}");
        }

        configurator.BindSingleton<T>();
        configurator.MultiBind<ModdableWeatherBase>().ToExisting<T>();

        return configurator;
    }

    public static Configurator BindWeather<T, TSettings> (this Configurator configurator)
        where T : ModdableWeatherBase, IWeatherWithSettings<TSettings>
        where TSettings : WeatherSettingEntry
    {
        configurator.BindWeather<T>();
        configurator.BindSingleton<TSettings>();
        configurator.MultiBind<IWeatherWithSettings>().ToExisting<T>();

        return configurator;
    }

    public static Configurator BindRainEffectWeather<T>(this Configurator configurator)
        where T : class, IRainEffectWeather
    {
        configurator.MultiBind<IRainEffectWeather>().ToExisting<T>();
        return configurator;
    }

}

