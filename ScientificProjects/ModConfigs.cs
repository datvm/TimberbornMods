global using ScientificProjects.Specs;
global using ScientificProjects.UI;
global using ScientificProjects.Management;

namespace ScientificProjects;

[Context("Game")]
public class ModGameConfig : Configurator
{
    public override void Configure()
    {
        Bind<ScientificProjectRegistry>().AsSingleton();
        Bind<ScientificProjectService>().AsSingleton();

        Bind<ScientificProjectScreen>().AsSingleton();
        
        MultiBind<IProjectCostProvider>().To<ModProjectCostProvider>().AsSingleton();
    }
}

public class ModStarter : IModStarter
{

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        var h = new Harmony(nameof(ScientificProjects));
        h.PatchAll();
    }

}