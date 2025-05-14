namespace BrainPowerSPs;

[Context("Game")]
public class ModGameConfig : Configurator
{
    public override void Configure()
    {
        Bind<PowerBuffs>().AsSingleton();
        Bind<SparePowerConverter>().AsSingleton();

        MultiBind<IProjectCostProvider>().To<ProjectsCostProvider>().AsSingleton();

        Bindito.Core.UiBuilderExtensions.MultiBindAndBindSingleton<IPrefabGroupServiceFrontRunner, PrefabModifier>(this);

        this.BindTrackingEntities()
            .TrackWorkplace()
            .Track<WaterPoweredGenerator>()
            .Track<WindPoweredGeneratorSpec>();

        Bindito.Core.UiBuilderExtensions.BindTemplateModule(this)
            .AddDecorator<WaterPoweredGenerator, WaterWheelBuffComponent>()
            .AddDecorator<WindPoweredGeneratorSpec, WindmillBuffComponent>()
            .Bind();
    }
}

public class ModStarter : IModStarter
{

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(BrainPowerSPs)).PatchAll();
    }

}