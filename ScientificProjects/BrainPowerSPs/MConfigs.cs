global using BrainPowerSPs.Mangement;
global using ScientificProjects.Management;
global using BrainPowerSPs.Buffs.Components;
global using BrainPowerSPs.Buffs;

namespace BrainPowerSPs;

[Context("Game")]
public class ModGameConfig : Configurator
{
    public override void Configure()
    {
        Bind<WaterWheelBuff>().AsSingleton();

        MultiBind<IProjectCostProvider>().To<ProjectsCostProvider>().AsSingleton();
        MultiBind<ITrackingEntities>().To<ModEntityTracker>().AsSingleton();

        MultiBind<TemplateModule>().ToProvider(() =>
        {
            TemplateModule.Builder b = new();

            b.AddDecorator<WaterPoweredGenerator, WaterWheelBuffComponent>();

            return b.Build();
        }).AsSingleton();
    }
}

public class ModStarter : IModStarter
{

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(BrainPowerSPs)).PatchAll();
    }

}