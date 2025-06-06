namespace ModdableWeather;

[Context("Bootstrapper")]
public class ModBootstrapperConfig : Configurator
{
    public override void Configure()
    {
        this.TryBindingModdableAudioClip();
    }
}

[Context("MainMenu")]
public class ModMenuConfig : Configurator
{

    public override void Configure()
    {
        this
            .BindSingleton<ModdableWeatherSpecService>()
            .BindSingleton<ModdableWeatherProfileSettings>()
            .TryBindingSystemFileDialogService()

            .BindDifficultyButtons()
            .BindModdedWeathers(true)
        ;

        Bind<ModdableWeatherProfileElement>().AsTransient();
        MultiBind<IModSettingElementFactory>().To<ModdableWeatherSettingsProfileFactory>().AsSingleton();
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
        this.MassRebind(h => h
            .Replace<WeatherService, ModdableWeatherService>()
            .Replace<HazardousWeatherService, ModdableHazardousWeatherService>()
            .Replace<HazardousWeatherApproachingTimer, ModdableHazardousWeatherApproachingTimer>()
            .Replace<HazardousWeatherNotificationPanel, ModdableHazardousWeatherNotificationPanel>()
            .Replace<DatePanel, ModdableDatePanel>()
            .Replace<WeatherPanel, ModdableWeatherPanel>()
            .Replace<Sun, ModdableSun>()
            .Replace<DayStageCycle, ModdableDayStageCycle>()

            .Remove<HazardousWeatherSoundPlayer>()
        );
            
        this.RemoveMultibinding<ICycleDuration>();
        this.MultiBindAndBindSingleton<ICycleDuration, ModdableWeatherCycleService>();

        this.BindSingleton<ModdableHazardousWeatherSoundPlayer>();

        // New Services
        this
            .BindDifficultyButtons()
            .BindSingleton<ModdableWeatherSpecService>()
            .BindSingleton<ModdableWeatherRegistry>()
            .BindSingleton<ModdableWeatherHistoryProvider>()
            .BindSingleton<ModdableWeatherGenerator>()
        ;
    }

}
