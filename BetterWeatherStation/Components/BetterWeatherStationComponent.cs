namespace BetterWeatherStation.Components;

[AddTemplateModule2(typeof(WeatherStation))]
public class BetterWeatherStationComponent(WeatherStationInfoService service) : BaseComponent, IInitializableEntity, IPersistentEntity,
    IDuplicable<BetterWeatherStationComponent>
{
    static readonly ComponentKey SaveKey = new(nameof(BetterWeatherStationComponent));
    static readonly ListKey<string> WeatherIdsKey = new("WeatherIds");

    bool loaded;

#nullable disable
    public WeatherStation WeatherStation { get; private set; }
    Automator automator;
#nullable enable

    readonly HashSet<string> weatherIds = [];
    public IReadOnlyCollection<string> WeatherIds => weatherIds;

    public bool EarlyHazardEnabled => WeatherStation.EarlyActivationEnabled;
    public int EarlyHazardHours => WeatherStation.EarlyActivationHours;

    public void InitializeEntity()
    {
        WeatherStation = GetComponent<WeatherStation>();
        automator = GetComponent<Automator>();

        if (weatherIds.Count == 0 && !loaded) // Try to migrate
        {
            weatherIds.Add(service.GetOrDefault(WeatherStation.Mode).Id);
        }
    }

    public void Save(IEntitySaver entitySaver)
    {
        var s = entitySaver.GetComponent(SaveKey);
        s.Set(WeatherIdsKey, WeatherIds);
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s)) { return; }

        weatherIds.Clear();
        weatherIds.UnionWith(s.Get(WeatherIdsKey));

        loaded = true;
    }

    public void Sample()
    {
        automator.SetState(ShouldEnable());
    }

    public bool ShouldEnable()
    {
        var status = service.CurrentWeatherStatus;

        if (weatherIds.Contains(status.CurrentStage.WeatherId))
        {
            return true;
        }

        if (EarlyHazardEnabled)
        {
            var nextWeather = status.NextStage;
            if (nextWeather.WeatherId is { } nextId && weatherIds.Contains(nextId))
            {
                return nextWeather.IsBenign == false && status.HoursToNextStage <= EarlyHazardHours;
            }
        }

        return false;
    }

    public void ToggleWeather(string id, bool enabled)
    {
        if (enabled)
        {
            if (!weatherIds.Add(id)) { return; }
        }
        else
        {
            if (!weatherIds.Remove(id)) { return; }
        }

        Sample();
    }

    public void SetWeathers(IEnumerable<string> ids)
    {
        weatherIds.Clear();
        weatherIds.UnionWith(ids);
        Sample();
    }

    public void DuplicateFrom(BetterWeatherStationComponent source)
    {
        SetWeathers(source.WeatherIds);
    }
}
