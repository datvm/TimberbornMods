global using WeatherScientificProjects.Management;
global using WeatherScientificProjects.Processors;
global using WeatherScientificProjects.UI;
global using WeatherScientificProjects.Helpers;

namespace WeatherScientificProjects;

[Context("Game")]
public class ModGameConfig : Configurator
{

    public override void Configure()
    {
        this
            .BindSingleton<WeatherUpgradeProcessor>()
            .BindSingleton<WeatherForecastPanel>()
            .BindSingleton<WeatherBuff>()
            .BindSingleton<ForecastHazardousWeatherApproachingTimerModifier>()

            .MultiBindSingleton<ITrackingEntities, WaterSourceTracking>()
            .MultiBindSingleton<IProjectCostProvider, WeatherProjectsCostProvider>()
            .MultiBindSingleton<IProjectUnlockConditionProvider, ModProjectUnlockConditionProvider>()

            .BindTemplateModule(h => h
                .AddDecorator<WaterSourceContamination, WeatherUpgradeWaterStrengthModifier>()
            );
        ;
    }

}
