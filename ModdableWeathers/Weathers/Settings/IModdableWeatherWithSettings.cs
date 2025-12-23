namespace ModdableWeathers.Weathers.Settings;

public interface IModdableWeatherWithSettings : IModdableWeather
{
    IModdableWeatherSettings Settings { get; }

    void ReloadSettings();
}

public interface IModdableWeatherWithSettings<TSetting> : IModdableWeatherWithSettings
    where TSetting : IModdableWeatherSettings
{
    IModdableWeatherSettings IModdableWeatherWithSettings.Settings => Settings;
    new TSetting Settings { get; }
}