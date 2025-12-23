namespace ModdableWeathers.Helpers;

public static class ModdableWeatherBindingExtensions
{

    public static Configurator BindWeather<T>(this Configurator configurator)
        where T : class, IModdableWeather
    {
        var isBenign = typeof(IModdableBenignWeather).IsAssignableFrom(typeof(T));
        var isHazardous = typeof(IModdableHazardousWeather).IsAssignableFrom(typeof(T));

        if (!isBenign && !isHazardous)
        {
            throw new InvalidOperationException($"{typeof(T).Name} must implement either {nameof(IModdableBenignWeather)} or {nameof(IModdableHazardousWeather)}");
        }

        configurator.BindSingleton<T>();
        configurator.MultiBind<IModdableWeather>().ToExisting<T>();

        return configurator;
    }

    public static Configurator BindWeather<T, TSettings>(this Configurator configurator)
        where T : class, IModdableWeatherWithSettings<TSettings>
        where TSettings : class, IModdableWeatherSettings
    {
        configurator.BindWeather<T>();

        configurator.BindSingleton<TSettings>();
        configurator.MultiBind<IModdableWeatherSettings>().ToExisting<TSettings>();

        return configurator;
    }

    public static Configurator BindWeatherModifier<T, TSettings>(this Configurator configurator)
        where T : class, IModdableWeatherModifier
        where TSettings : ModdableWeatherModifierSettings
    {
        configurator.BindSingleton<T>();
        configurator.MultiBind<IModdableWeatherModifier>().ToExisting<T>();

        configurator.BindSingleton<TSettings>();
        configurator.MultiBind<ModdableWeatherModifierSettings>().ToExisting<TSettings>();
        return configurator;
    }

}
