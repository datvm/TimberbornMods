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

            .MultiBindSingleton<IPrefabModifier, PrefabModifier>()

            .BindFragment<DamGateFragment>()
            .BindFragment<FloodgateAutoFragment>()
            .BindSingleton<SluiceUpstreamFragment>() // This is not a fragment

            .BindTemplateModule(h => h
                .AddDecorator<DamGateComponentSpec, DamGateComponent>()
                .AddDecorator<FloodgateSpec, FloodgateAutoComponent>()
                
                .AddDecorator<Sluice, SluiceUpstreamComponent>()
                .AddDecorator<Sluice, SluiceUpstreamMarker>()
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
        new Harmony(nameof(HydroFormaProjects)).PatchAll();
    }

}
