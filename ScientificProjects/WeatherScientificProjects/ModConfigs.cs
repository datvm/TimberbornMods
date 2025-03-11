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
        Bind<WeatherBuff>().AsSingleton();

        MultiBind<IDevModule>().To<WeatherDevModule>().AsSingleton();
        MultiBind<ITrackingEntities>().To<WaterSourceTracking>().AsSingleton();
        MultiBind<IProjectCostProvider>().To<WeatherProjectsCostProvider>().AsSingleton();

        MultiBind<TemplateModule>().ToProvider(() =>
        {
            TemplateModule.Builder builder = new();
            builder.AddDecorator<WaterSourceContamination, WeatherUpgradeWaterStrengthModifier>();
            return builder.Build();
        }).AsSingleton();
    }

}
