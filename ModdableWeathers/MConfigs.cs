namespace ModdableWeathers;

public class ModdableWeatherConfig : BaseModdableTimberbornConfigurationWithHarmony
{
    public static readonly ImmutableArray<Type> AssemblyTypes = [.. typeof(ModdableWeatherConfig).Assembly.GetTypes()];

    public override ConfigurationContext AvailableContexts { get; } = ConfigurationContext.Game | ConfigurationContext.Bootstrapper;

    public override void StartMod(IModEnvironment modEnvironment)
    {
        base.StartMod(modEnvironment);
        AttributePatcher.Patches();
    }

    public override void Configure(Configurator configurator, ConfigurationContext context)
    {
        if (context == ConfigurationContext.Bootstrapper)
        {
            BindBootstrapperServices(configurator);
            return;
        }

        ReplaceServices(configurator);

        configurator
            // Audio
            .TryBindingAudioClipManagement()

            // History
            .BindSingleton<WeatherCycleStageDefinitionService>()
            .BindSingleton<WeatherHistoryRegistry>()
            .BindSingleton<WeatherHistoryService>()

            // Cycles
            .MultiBindAndBindSingleton<ICycleDuration, CycleDurationService>()
            .BindSingleton<WeatherGenerator>()
            .BindSingleton<WeatherCycleService>()

            // Weather Services
            .BindSingleton<ModdableWeatherSpecService>()
            .BindSingleton<ModdableWeatherRegistry>()
            .BindSingleton<ModdableWeatherSettingsService>()

            // Weather Modifiers
            .BindSingleton<ModdableWeatherModifierSpecService>()
            .BindSingleton<ModdableWeatherModifierRegistry>()
            .BindSingleton<ModdableWeatherModifierSettingsService>()

            // Game Built-in Weathers
            .BindWeather<GameTemperateWeather, GameTemperateWeatherSettings>()
            .BindWeather<GameDroughtWeather, GameDroughtWeatherSettings>()
            .BindWeather<GameBadtideWeather, GameBadtideWeatherSettings>()

            // Mod built-in Weathers
            .BindWeather<RainModdableWeather, RainModdableWeatherSettings>()
            .BindWeather<MonsoonModdableWeather, MoonsoonWeatherSettings>()
            .BindWeather<SurprisinglyRefreshingModdableWeather, SurprisinglyRefreshingWeatherSettings>()

            // Mod built-in Modifiers
            .BindWeatherModifier<DroughtModifier, DroughtModifierSettings>()
            .BindWeatherModifier<BadtideModifier, BadtideModifierSettings>()
            .BindWeatherModifier<MonsoonModifier, MonsoonModifierSettings>()
            .BindWeatherModifier<RefreshingModifier, RefreshingModifierSettings>()
            .BindWeatherModifier<RainModifier, RainModifierSettings>()

            // Special weathers, only bind, don't multibind
            .BindSingleton<EmptyBenignWeather>()
            .BindSingleton<EmptyHazardousWeather>()

            // Settings Dialog            
            .BindSingleton<WatherSettingsExportService>()
            .BindSingleton<WeatherSettingsDialogShower>()
            .BindTransient<WeatherSettingsDialog>()            
            .BindTransient<SettingElement>()
            .BindTransient<WeatherSettingsPanel>()
            .BindTransient<WeatherModifierSettingsPanel>()
            .BindTransient<WeatherModifierAssociationPanel>()
            .BindTransient<WeatherSettingsExportPanel>()

            // Rain effect
            .BindSingleton<RainSettings>()
            .BindSingleton<RainEffectPlayer>()

            // Water & Land Modifiers
            .BindSingleton<ModdableWaterStrengthModifierService>()
            .BindSingleton<ModdableWaterContaminationModifierService>()
            .BindSingleton<LandContaminationBlockerService>()

            // Wetfur applier
            .BindSingleton<SoakEffectApplierService>()

            // Components
            .BindTemplateModule(h => h
                .AddDecorator<WaterSource, ModdableWaterStrengthModifier>()
                .AddDecorator<WaterSourceContaminationSpec, ModdableWaterSourceContaminationController>()

                .AddDecorator<NeedManager, WeatherSoakEffectApplier>()
            )
        ;
    }

    void BindBootstrapperServices(Configurator configurator)
    {
        configurator
            .TryBindingSystemFileDialogService()
        ;
    }

    void ReplaceServices(Configurator configurator)
    {
        configurator.RemoveMultibinding<ICycleDuration>();

        List<Type> removingTypes = [];
        List<KeyValuePair<Type, Type>> replacingTypes = [];

        foreach (var t in AssemblyTypes)
        {
            var replacing = t.GetCustomAttribute<ReplaceSingletonAttribute>();
            if (replacing is null) { continue; }

            var replaced = replacing.Replaced ?? t.BaseType;
            removingTypes.Add(replaced);
            replacingTypes.Add(new(replaced, t));
        }

        configurator.MassRemoveBindings(removingTypes);
        foreach (var (from, to) in replacingTypes)
        {
            configurator.BindSingleton(from, to, true);
        }
    }

}
