using ModdableWeather.HazardousTimer;
using ModdableWeather.Services.Registries;

namespace ModdableWeather;

public class ModdableWeatherConfigs : BaseModdableTimberbornConfigurationWithHarmony
{
    public override ConfigurationContext AvailableContexts { get; } = ConfigurationContext.Bootstrapper | ConfigurationContext.Game;

    public override void StartMod(IModEnvironment modEnvironment)
    {
        base.StartMod(modEnvironment);

        ScanForAttributes(new Harmony(nameof(ModdableWeather)));
    }

    public override void Configure(Configurator configurator, ConfigurationContext context)
    {
        switch (context)
        {
            case ConfigurationContext.Bootstrapper:
                configurator.TryBindingModdableAudioClip();
                break;
            case ConfigurationContext.Game:
                BindGame(configurator);
                break;
            case ConfigurationContext.MapEditor:
                throw new InvalidOperationException("Please disable Moddable Weather mod to use Map Editor.");
        }
    }

    void BindGame(Configurator config)
    {
        BindWeathers(config);

        // Replace/remove Services
        config.MassRebind(h => h
            .Replace<WeatherService, ModdableWeatherService>()
            .Replace<HazardousWeatherService, ModdableHazardousWeatherService>()
            .Replace<HazardousWeatherNotificationPanel, ModdableHazardousWeatherNotificationPanel>()
            .Replace<DatePanel, ModdableDatePanel>()
            .Replace<WeatherPanel, ModdableWeatherPanel>()
            .Replace<Sun, ModdableSun>()
            .Replace<DayStageCycle, ModdableDayStageCycle>()

            .Remove<HazardousWeatherSoundPlayer>()
        );

        config.RemoveMultibinding<ICycleDuration>();
        config.MultiBindAndBindSingleton<ICycleDuration, ModdableWeatherCycleService>();

        config.BindSingleton<ModdableHazardousWeatherSoundPlayer>();

        // New Services
        config
            .BindSingleton<ModdableWeatherSpecRegistry>()
            .BindSingleton<ModdableWeatherRegistry>()
            .BindSingleton<ModdableWeatherHistoryProvider>()
            .BindSingleton<ModdableWeatherGenerator>()
            .BindSingleton<ApproachingTimerModifierService>()
        ;

        config.MultiBindSingleton<IDevModule, ModdableWeatherDevModule>();
    }

    void BindWeathers(Configurator config)
    {
        config
            // Don't multibind these
            .BindSingleton<EmptyBenignWeather>()
            .BindSingleton<EmptyHazardousWeather>()

            .BindWeather<GameTemperateWeather, GameTemperateWeatherSettings>()
            .BindWeather<GameDroughtWeather, GameDroughtWeatherSettings>()
            .BindWeather<GameBadtideWeather, GameBadtideWeatherSettings>()            
        ;
    }

    public static void ScanForAttributes(Harmony harmony)
    {
        var types = Assembly.GetExecutingAssembly()
            .DefinedTypes
            .Select(q => (q, q.GetCustomAttribute<HarmonyPatch>()))
            .Where(q => q.Item2?.info.declaringType is not null);

        foreach (var (type, attr) in types)
        {
            if (!(type.IsAbstract && type.IsSealed))
            {
                throw new InvalidOperationException(
                    $"Type {type} is not static. Harmony patching requires static classes.");
            }

            var originalType = attr.info.declaringType;
            var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public);
            foreach (var method in methods)
            {
                PatchGetter(harmony, method, originalType);
            }
        }
    }

    static void PatchGetter(Harmony harmony, MethodInfo method, Type original)
    {
        var getterAttr = method.GetCustomAttribute<HarmonyGetterPatchAttribute>();
        if (getterAttr is null) { return; }

        var propGetter = original.PropertyGetter(getterAttr.Name)
            ?? throw new InvalidOperationException(
                $"Property {getterAttr.Name} not found on {original}");

        harmony.Patch(propGetter, prefix: method);
    }
}