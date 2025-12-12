using ModdableWeather.Services.Registries;

namespace ModdableWeather.UI;

public class ModdableHazardousWeatherSoundPlayer(
    EventBus eb,
    ModdableWeatherSpecRegistry specs,
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

    void PlayWeatherSound(IModdableWeather weather)
    {        
        var sound = GetSoundName(weather);
        sounds.PlaySound2D(sound);
    }

    string GetSoundName(IModdableWeather weather) =>
        weather.Spec.StartSound ?? (weather.AsCalm() is not null ?
        specs.DefaultTemperateSound : specs.DefaultDroughtSound);

}
