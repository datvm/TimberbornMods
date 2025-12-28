namespace ModdableWeathers.UI;

[ReplaceSingleton]
[BypassMethods([
    nameof(OnHazardousWeatherStarted),
    nameof(OnHazardousWeatherEnded),
])]
public class ModdableGameMusicPlayer(ISoundSystem soundSystem, IRandomNumberGenerator randomNumberGenerator, WeatherService weatherService, EventBus eventBus, RootObjectProvider rootObjectProvider, ISpecService specService) : GameMusicPlayer(soundSystem, randomNumberGenerator, weatherService, eventBus, rootObjectProvider, specService)
{

    [OnEvent]
    public void OnWeatherTransitioned(WeatherTransitionedEvent e)
    {
        var isHazardous = e.To.Weather.IsHazardous;

        if (isHazardous)
        {
            StopStandardMusic();
            StartDroughtMusic();
        }
        else
        {
            StopDroughtMusic();
            StartStandardMusic();
        }
    }

}
