namespace ModdableWeather;

[Context("MainMenu")]
public class ModMenuConfig : Configurator
{

    public override void Configure()
    {
        this
            .BindSingleton<ModdableWeatherSpecService>()
            .BindModdedWeathers(true)
            .BindModdedAudioClips();
    }

}

[Context("Game")]
public class ModGameConfig : Configurator
{

    public override void Configure()
    {
        // Weathers
        this.BindModdedWeathers(false);

        // Replace/remove Services
        this.MassRebind()
            .Replace<WeatherService, ModdableWeatherService>()
            .Replace<HazardousWeatherService, ModdableHazardousWeatherService>()
            .Replace<HazardousWeatherApproachingTimer, ModdableHazardousWeatherApproachingTimer>()
            .Replace<HazardousWeatherNotificationPanel, ModdableHazardousWeatherNotificationPanel>()
            .Replace<DatePanel, ModdableDatePanel>()
            .Replace<WeatherPanel, ModdableWeatherPanel>()
            .Replace<Sun, ModdableSun>()
            .Replace<DayStageCycle, ModdableDayStageCycle>()

            .Remove<HazardousWeatherSoundPlayer>()
        .Bind();
            
        this.RemoveMultibinding<ICycleDuration>();
        this.MultiBindAndBindSingleton<ICycleDuration, ModdableWeatherCycleService>();

        this.BindSingleton<ModdableHazardousWeatherSoundPlayer>();

        // New Services
        this
            .BindSingleton<ModdableWeatherSpecService>()
            .BindSingleton<ModdableWeatherRegistry>()
            .BindSingleton<ModdableWeatherHistoryProvider>()
            .BindSingleton<ModdableWeatherGenerator>()

            // Bind but do NOT multibind this special weather for record keeping
            .BindSingleton<NoneHazardousWeather>()
            // Modded Sounds
            .BindModdedAudioClips();
    }

}
