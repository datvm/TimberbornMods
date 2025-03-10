global using WeatherScientificProjects.Management;
global using WeatherScientificProjects.Processors;

namespace WeatherScientificProjects;

[Context("Game")]
public class ModGameConfig : Configurator
{
    
    public override void Configure()
    {
        Bind<WarningExtensionProcessor>().AsSingleton();

        MultiBind<IDevModule>().To<WeatherDevModule>().AsSingleton();
        MultiBind<ITrackingEntities>().To<WaterSourceTracking>().AsSingleton();
        MultiBind<IProjectCostProvider>().To<WeatherProjectsCostProvider>().AsSingleton();
    }

}
