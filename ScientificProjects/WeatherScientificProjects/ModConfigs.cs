global using WeatherScientificProjects.Management;
global using WeatherScientificProjects.Processors;
global using WeatherScientificProjects.UI;

namespace WeatherScientificProjects;

[Context("Game")]
public class ModGameConfig : Configurator
{
    
    public override void Configure()
    {
        Bind<WeatherUpgradeProcessor>().AsSingleton();
        Bind<WeatherForecastPanel>().AsSingleton();

        MultiBind<IDevModule>().To<WeatherDevModule>().AsSingleton();
        MultiBind<ITrackingEntities>().To<WaterSourceTracking>().AsSingleton();
        MultiBind<IProjectCostProvider>().To<WeatherProjectsCostProvider>().AsSingleton();
    }

}

public class ModStarter : IModStarter
{
    public void StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(WeatherScientificProjects)).PatchAll();
    }
}