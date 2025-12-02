namespace HydroFormaProjects;

public class MGameConfigs : BaseModdableTimberbornConfigurationWithHarmony, IWithDIConfig
{
    public override ConfigurationContext AvailableContexts { get; } = ConfigurationContext.Game;

    public override void StartMod(IModEnvironment modEnvironment)
    {
        base.StartMod(modEnvironment);

        new Harmony(nameof(HydroFormaProjects)).PatchHangingTerrains();

        ModdableTimberbornRegistry.Instance
            .UseEntityTracker()
            .TryTrack<FloodgateAutoComponent>();
    }

    public override void Configure(Configurator configurator, ConfigurationContext context)
    {
        configurator
            .BindScientificProjectListener<DamGateService>(true)
            .BindScientificProjectListener<FloodgateAutoService>(true)
            .BindScientificProjectListener<SluiceUpstreamService>(true)
            .BindScientificProjectListener<StreamGaugeSensorService>(true)

            .BindTemplateModifier<HFSPTemplateModifier>()
            .BindTemplateTailRunner<TerrainBlockUpgradeService>()

            .BindFragment<DamGateFragment>()
            .BindFragment<FloodgateAutoFragment>()
            .BindSingleton<SluiceUpstreamFragment>() // This is not a fragment
            .BindFragment<StreamGaugeUpgradeFragment>()

            .BindTemplateModule(h => h
                .AddDecorator<DamGateComponentSpec, DamGateComponent>()
                .AddDecorator<FloodgateSpec, FloodgateAutoComponent>()

                .AddDecorator<Sluice, SluiceUpstreamComponent>()
                .AddDecorator<Sluice, SluiceUpstreamMarker>()

                .AddDecorator<RecipeTimeMultiplierSpec, RecipeTimeMultiplier>()

                .AddDecorator<StreamGaugeSpec, StreamGaugeSensor>()
            )
        ;
    }

}
