namespace HydroFormaProjects;

[Context("Game")]
public class ModGameConfig : Configurator
{
    public override void Configure()
    {
        this
            .BindSingleton<DamGateService>()
            .BindSingleton<FloodgateAutoService>()
            .BindSingleton<SluiceUpstreamService>()
            .BindSingleton<StreamGaugeSensorService>()

            .MultiBindSingleton<IPrefabModifier, PrefabModifier>()
            .MultiBindSingleton<IPrefabGroupServiceFrontRunner, TerrainBlockUpgradeService>()

            .BindFragment<DamGateFragment>()
            .BindFragment<FloodgateAutoFragment>()
            .BindSingleton<SluiceUpstreamFragment>() // This is not a fragment

            .BindTemplateModule(h => h
                .AddDecorator<DamGateComponentSpec, DamGateComponent>()
                .AddDecorator<FloodgateSpec, FloodgateAutoComponent>()

                .AddDecorator<Sluice, SluiceUpstreamComponent>()
                .AddDecorator<Sluice, SluiceUpstreamMarker>()

                .AddDecorator<RecipeTimeMultiplierSpec, RecipeTimeMultiplier>()

                .AddDecorator<StreamGaugeSpec, StreamGaugeSensor>()
            )
        ;

        this.BindTrackingEntities()
            .Track<FloodgateAutoComponent>();
    }
}

public class ModStarter : IModStarter
{

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(HydroFormaProjects))
            .PatchHangingTerrains()
            .PatchAll();
    }

}
