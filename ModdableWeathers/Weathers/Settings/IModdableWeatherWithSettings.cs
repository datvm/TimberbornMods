namespace ModdableWeathers.Weathers.Settings;

public interface IModdableWeatherWithSettings : IModdableWeather { }

public interface IModdableWeatherWithSettings<TSetting> : IModdableWeatherWithSettings
    where TSetting : IModdableWeatherSettings, new()
{
    TSetting Settings { get; }
}