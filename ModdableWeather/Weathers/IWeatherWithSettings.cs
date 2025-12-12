namespace ModdableWeather.Weathers;

public interface IWeatherWithSettings
{
    Type SettingsType { get; }
}

public interface IWeatherWithSettings<TSettings> : IWeatherWithSettings
    where TSettings : WeatherSettingEntry
{
    Type IWeatherWithSettings.SettingsType => typeof(TSettings);

    TSettings Settings { get; }
}