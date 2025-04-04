global using BrainPowerSPs.Mangement;
global using ScientificProjects.Management;
global using BrainPowerSPs.Buffs.Components;
global using BrainPowerSPs.Buffs;
global using BrainPowerSPs.Patches;

namespace BrainPowerSPs;

[Context("Game")]
public class ModGameConfig : Configurator
{
    public override void Configure()
    {
        Bind<PowerBuffs>().AsSingleton();
        Bind<SparePowerConverter>().AsSingleton();

        MultiBind<IProjectCostProvider>().To<ProjectsCostProvider>().AsSingleton();

        this.MultiBindAndBindSingleton<IPrefabGroupServiceFrontRunner, PrefabModifier>();

        this.BindTrackingEntities()
            .TrackWorkplace()
            .Track<WaterPoweredGenerator>()
            .Track<WindPoweredGeneratorSpec>();

        this.BindTemplateModule()
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