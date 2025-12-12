namespace ModdableWeather.Settings;

public class WeatherSettingsService(
    ISingletonLoader loader,
    PersistentGameModeService persistentGameModeService,
    IEnumerable<WeatherSettingEntry> entries
) : ILoadableSingleton, ISaveableSingleton
{
    static readonly SingletonKey SaveKey = new(nameof(ModdableWeather) + "." + nameof(WeatherSettingsService));
    static readonly PropertyKey<string> EntriesKey = new("Entries");

    readonly ImmutableArray<WeatherSettingEntry> entries = [.. entries];

    public FrozenDictionary<string, WeatherSettingEntry> EntriesByType { get; private set; } = FrozenDictionary<string, WeatherSettingEntry>.Empty;

    public TSettings Get<TSettings>(ModdableWeatherWithSettings<TSettings> weather)
        where TSettings : WeatherSettingEntry
    {
        if (!EntriesByType.TryGetValue(weather.Id, out var entry))
        {
            throw new InvalidOperationException($"No settings entry registered for weather ID {weather.Id}.");
        }

        if (entry is not TSettings typedEntry)
        {
            throw new InvalidOperationException($"Settings entry for weather ID {weather.Id} is of type {entry.GetType().FullName}, " +
                $"but requested as type {typeof(TSettings).FullName}.");
        }

        return typedEntry;
    }

    public void Load()
    {
        var gameMode = persistentGameModeService.ReconstructedMode;

        var rawValues = LoadRawValues();
        Dictionary<string, WeatherSettingEntry> entriesByType = [];

        foreach (var entry in entries)
        {
            var id = entry.EntityId;

            if (entriesByType.ContainsKey(id))
            {
                throw new InvalidOperationException($"Both {entriesByType[id].GetType().FullName} and {entry.GetType().FullName} " +
                    $"are registered for weather ID {id}.");
            }

            entriesByType[id] = entry;

            if (rawValues.TryGetValue(id, out var json))
            {
                entry.SetValue(json);
            }
            else
            {
                entry.SetValueForDifficulty(gameMode);
            }
        }

        EntriesByType = entriesByType.ToFrozenDictionary();
    }

    Dictionary<string, JToken> LoadRawValues()
    {
        if (!loader.TryGetSingleton(SaveKey, out var s)
            || !s.Has(EntriesKey))
        {
            return [];
        }

        var raw = s.Get(EntriesKey);
        return JsonConvert.DeserializeObject<Dictionary<string, JToken>>(raw) ?? [];
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        var s = singletonSaver.GetSingleton(SaveKey);

        var rawValues = entries
            .ToDictionary(q => q.EntityId, q => q.ToJson());

        s.Set(EntriesKey, JsonConvert.SerializeObject(rawValues));
    }

}
