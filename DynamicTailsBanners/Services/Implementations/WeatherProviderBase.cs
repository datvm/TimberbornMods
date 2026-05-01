namespace DynamicTailsBanners.Services.Implementations;

public abstract class WeatherProviderBase<TComp>(
    IAssetLoader assets,
    WeatherIdService weatherIdService,
    EventBus eb
) : ILoadableSingleton
    where TComp : IDynamicDecalComponent
{

    public abstract string Id { get; }
    const string DefaultId = "TemperateWeather";

    string? currId;
    Texture2D? currTexture;

    readonly HashSet<TComp> comps = [];

    protected abstract string GetWeatherPath(string id);

    public void Load()
    {
        eb.Register(this);
    }

    public Texture2D GetTexture(TComp _)
    {
        var weatherId = weatherIdService.GetId();
        if (currId != weatherId)
        {
            currId = weatherId;
            currTexture = LoadOrDefault(currId);
        }

        return currTexture ?? Texture2D.whiteTexture;
    }

    Texture2D LoadOrDefault(string id)
    {
        var texture = assets.LoadSafe<Texture2D>(GetWeatherPath(id));

        if (texture is null)
        {
            return id == DefaultId
                ? throw new InvalidOperationException($"Default weather tail texture with id '{DefaultId}' is missing.")
                : LoadOrDefault(DefaultId);
        }

        return texture;
    }

    public void Register(TComp comp) => comps.Add(comp);
    public void Unregister(TComp comp) => comps.Remove(comp);

    [OnEvent]
    public void OnHazWeather(HazardousWeatherStartedEvent _) => OnNewWeather();

    [OnEvent]
    public void OnNewWeather(CycleStartedEvent _) => OnNewWeather();

    void OnNewWeather()
    {
        GetTexture(default!);

        foreach (var comp in comps)
        {
            comp.ShowTexture();
        }
    }

}
