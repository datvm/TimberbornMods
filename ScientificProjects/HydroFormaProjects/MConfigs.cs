namespace HydroFormaProjects;

public class MGameConfigs : BaseModdableTimberbornConfigurationWithHarmony
{

    public override void StartMod(IModEnvironment modEnvironment)
    {
        base.StartMod(modEnvironment);

        new Harmony(nameof(HydroFormaProjects)).PatchHangingTerrains();

        ModdableTimberbornRegistry.Instance            
            .UseDependencyInjection()
            .UseEntityTracker()
            .TryTrack<FloodgateAutoComponent>();
    }

    public override void Configure(Configurator configurator, ConfigurationContext context)
    {
        if (!context.IsGameContext()) { return; }

        configurator
            .BindScientificProjectListener<DamGateService>(true)
            .BindScientificProjectListener<FloodgateAutoService>(true)
            .BindScientificProjectListener<SluiceUpstreamService>(true)
            .BindScientificProjectListener<StreamGaugeSensorService>(true)

            .BindPrefabModifier<HFSPPrefabModifier>()
            .MultiBindSingleton<IPrefabGroupServiceTailRunner, TerrainBlockUpgradeService>()

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
