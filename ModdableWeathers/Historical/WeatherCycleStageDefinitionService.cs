namespace ModdableWeathers.Historical;

public class WeatherCycleStageDefinitionService
{

    public static readonly ImmutableArray<WeatherCycleStageDefinition> DefaultDefinitions = [
        new(0, 0, 100, 100),
        new(1, 0, 0, 100),
    ];

    public ImmutableArray<WeatherCycleStageDefinition> StagesDefinitions { get; set; } = DefaultDefinitions;

}
