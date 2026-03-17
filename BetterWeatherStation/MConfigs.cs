global using BetterWeatherStation.Services;
global using BetterWeatherStation.Components;
global using ModdableTimberborn.BuildingSettings;

namespace BetterWeatherStation;

public class BetterWeatherStationConfigs : BaseModdableTimberbornConfigurationWithHarmony
{
    public override ConfigurationContext AvailableContexts { get; } = ConfigurationContext.Game;

    public override void Configure(Configurator configurator, ConfigurationContext context)
    {
        configurator.BindAttributes(context);
    }

}
