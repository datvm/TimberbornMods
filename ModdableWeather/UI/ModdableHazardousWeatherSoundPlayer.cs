namespace ModdableWeather.UI;

public class ModdableHazardousWeatherSoundPlayer(
    EventBus eb,
    ModdableWeatherSpecService specs,
    GameUISoundController sounds
) : ILoadableSingleton
{

    public void Load()
    {
        eb.Register(this);
    }

    [OnEvent]
    public void OnCycleWeatherDecided(OnModdableWeatherCycleDecided ev)
    {
        PlayWeatherSound(ev.WeatherCycle.TemperateWeather);
    }

    [OnEvent]
    public void OnHazardousWeatherStarted(HazardousWeatherStartedEvent ev)
    {
        PlayWeatherSound(ev.GetWeather());
    }

    void PlayWeatherSound(IModdedWeather weather)
    {        
        var sound = GetSoundName(weather);
        sounds.PlaySound2D(sound);
    }

    string GetSoundName(IModdedWeather weather) =>
        weather.Spec.StartSound ?? (weather.IsTemperate() is not null ?
        specs.DefaultTemperateSound : specs.DefaultDroughtSound);

}
