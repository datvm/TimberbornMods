namespace ModdableWeather.Helpers;

public static class ModdableWeatherUtils
{

    internal static SingletonKey SaveKey = new(nameof(ModdableWeather));

    public static readonly bool HasMoreModLog = AppDomain.CurrentDomain.GetAssemblies().Any(q => q.GetName().Name == "MoreModLogs");

    public static void Log(Func<string> message)
    {
        if (HasMoreModLog)
        {
            Debug.Log($"{nameof(ModdableWeather)}: " + message());
        }
    }

    internal static Configurator BindModdedAudioClips(this Configurator configurator)
    {
        configurator.BindSingleton<IModFileConverter<AudioClip>, ModAudioClipConverter>();
        configurator.MultiBind<IAssetProvider>().To<ModSystemFileProvider<AudioClip>>().AsSingleton();

        return configurator;
    }

    internal static Configurator BindModdedWeathers(this Configurator configurator, bool menuContext)
    {
        // Game Default
        configurator
            .BindTemperateWeather<GameTemperateWeather, GameTemperateWeatherSettings>(menuContext)
            .BindHazardousWeather<GameDroughtWeather, GameDroughtWeatherSettings>(menuContext)
            .BindHazardousWeather<GameBadtideWeather, GameBadtideWeatherSettings>(menuContext);

        // Modded
        configurator
            .BindTemperateWeather<RainWeather, RainWeatherSettings>(menuContext);

        return configurator;
    }

}
