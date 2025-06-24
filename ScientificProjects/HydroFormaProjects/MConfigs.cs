namespace HydroFormaProjects;

[Context("Game")]
public class ModGameConfig : Configurator
{
    public override void Configure()
    {
        this
            .BindSingleton<DamGateService>()
            .BindSingleton<FloodgateAutoService>()

            .MultiBindSingleton<IPrefabModifier, PrefabModifier>()

            .BindFragment<DamGateFragment>()
            .BindFragment<FloodgateAutoFragment>()

            .BindTemplateModule(h => h
                .AddDecorator<DamGateComponentSpec, DamGateComponent>()
                .AddDecorator<FloodgateSpec, FloodgateAutoComponent>()
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
