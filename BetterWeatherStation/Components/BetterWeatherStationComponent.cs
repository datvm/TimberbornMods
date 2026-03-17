namespace BetterWeatherStation.Components;

[AddTemplateModule2(typeof(WeatherStation))]
public class BetterWeatherStationComponent(WeatherStationInfoService service) : BaseComponent, IAwakableComponent, IStartableComponent, IPersistentEntity,
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

    public void Awake()
    {
        WeatherStation = GetComponent<WeatherStation>();
        automator = GetComponent<Automator>();
    }

    public void Start()
    {
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
        var (curr, next, hours) = service.CurrentWeatherStatus;

        if (weatherIds.Contains(curr.Id))
        {
            return true;
        }

        if (weatherIds.Contains(next.Id))
        {
            return next.Hazardous && EarlyHazardEnabled && hours <= EarlyHazardHours;
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
