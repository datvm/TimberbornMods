namespace ModdableWeathers.UI;

public class ModdableWeatherSoundPlayer(
    EventBus eb,
    GameTemperateWeather temperateWeather,
    GameDroughtWeather droughtWeather,
    GameUISoundController sounds
) : ILoadableSingleton
{
    public void Load()
    {
        eb.Register(this);
    }

    [OnEvent]
    public void OnWeatherTransitioned(WeatherTransitionedEvent e) => PlayWeatherSound(e.To.Weather);

    public void PlayWeatherSound(IModdableWeather weather)
    {
        var sound = GetSoundName(weather);
        sounds.PlaySound2D(sound);
    }

    string GetSoundName(IModdableWeather weather) =>
        weather.Spec.StartSound
        ?? (weather.IsBenign ? temperateWeather.Spec.StartSound : droughtWeather.Spec.StartSound)
        ?? throw new InvalidOperationException("No start sound defined for weather " + weather.Id);
}
