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

    public static NewGameMode GetGameSettingsAtDifficulty(WeatherDifficulty difficulty)
    {
        return difficulty switch
        {
            WeatherDifficulty.Easy => NewGameModes.Easy(),
            WeatherDifficulty.Normal => NewGameModes.Normal(),
            WeatherDifficulty.Hard => NewGameModes.Hard(),
            _ => NewGameModes.Default(),
        };
    }

    internal static Configurator BindDifficultyButtons(this Configurator configurator)
    {
        configurator.MultiBind<IModSettingElementFactory>().To<WeatherDifficultyModSettingFactory>().AsSingleton();
        return configurator;
    }

    internal static Configurator BindModdedWeathers(this Configurator configurator, bool menuContext)
    {
        // Game Default weathers
        configurator
            .BindTemperateWeather<GameTemperateWeather, GameTemperateWeatherSettings>(menuContext)
            .BindHazardousWeather<GameDroughtWeather, GameDroughtWeatherSettings>(menuContext)
            .BindHazardousWeather<GameBadtideWeather, GameBadtideWeatherSettings>(menuContext)


            // Modded weathers
            .BindTemperateWeather<RainWeather, RainWeatherSettings>(menuContext)            
            .BindTemperateWeather<ShortTemperateWeather, ShortTemperateWeatherSettings>(menuContext)
            .BindTemperateWeather<ProgressiveTemperateWeather, ProgressiveTemperateWeatherSettings>(menuContext)
            .BindHazardousWeather<MonsoonWeather, MonsoonWeatherSettings>(menuContext)
            .BindHazardousWeather<SurprisinglyRefreshingWeather, SurprisinglyRefreshingWeatherSettings>(menuContext)

            // Single Weather Settings
            .BindSingleton<SingleWeatherModeSettings>()
        ;

        // Settings
        configurator.MultiBindSingleton<IModSettingElementFactory, LabelSettingFactory>();

        if (!menuContext)
        {
            configurator
                .BindHazardousWeather<NoneHazardousWeather>()
                .BindTemperateWeather<NoneTemperateWeather>();

            // Weather-specific services
            configurator.BindSingleton<RainEffect>();

            configurator.BindTemplateModule(h => h
                .AddDecorator<WaterSource, MonsoonWaterStrengthModifier>()
                .AddDecorator<WaterSourceContamination, SurprisinglyRefreshingController>()
                .AddDecorator<NeedManager, RainSoakedEffectApplier>()
            );

            // Rain effect
            configurator.BindRainEffectWeather<RainWeather>();
        }

        return configurator;
    }

    internal static ModSettingsBox CloseAndOpenAgain(this ModSettingsBox box, string modId, ModRepository modRepository)
    {
        var mod = modRepository.EnabledMods.FirstOrDefault(q => q.Manifest.Id == modId);
        if (mod is null) { return box; }

        box.Close();
        box.Open(mod);

        return box;
    }

    public static float CalculateHandicap(int counter, int handicapCycles, Func<int> getInitHandicapPercent)
    {
        if (counter >= handicapCycles || handicapCycles == 0) { return 1f; }

        var initHandicap = getInitHandicapPercent();
        var deltaPerCycle = (100 - initHandicap) / handicapCycles;

        var handicap = initHandicap + (deltaPerCycle * counter);
        return handicap / 100f;
    }

}
