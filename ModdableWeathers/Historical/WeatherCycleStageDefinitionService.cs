namespace ModdableWeathers.Historical;

public class WeatherCycleStageDefinitionService(ISingletonLoader loader) : ILoadableSingleton, ISaveableSingleton
{
    static readonly SingletonKey SaveKey = new(nameof(WeatherCycleStageDefinitionService));
    static readonly PropertyKey<string> StagesDefinitionsKey = new(nameof(StagesDefinitions));

    public static readonly ImmutableArray<WeatherCycleStageDefinition> DefaultDefinitions = [
        new(0, 0, 100, 100),
        new(1, 0, 0, 100),
    ];

    public ImmutableArray<WeatherCycleStageDefinition> StagesDefinitions { get; set; } = DefaultDefinitions;

    public void Load()
    {
        if (!loader.TryGetSingleton(SaveKey, out var s)) { return; }

        if (s.Has(StagesDefinitionsKey))
        {
            var savedDefinitions = JsonConvert.DeserializeObject<WeatherCycleStageDefinition[]>(s.Get(StagesDefinitionsKey));
            if (savedDefinitions is not null)
            {
                StagesDefinitions = [.. savedDefinitions];
            }
        }
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        var s = singletonSaver.GetSingleton(SaveKey);
        s.Set(StagesDefinitionsKey, JsonConvert.SerializeObject(StagesDefinitions));
    }

}
