namespace DynamicTailsBanners.Services.Implementations;

[MultiBind(typeof(IDynamicDecalProvider))]
public class WeatherDynamicTailProvider(
    IAssetLoader assets,
    WeatherIdService weatherIdService,
    EventBus eb
) : IDynamicTailDecalProvider, ILoadableSingleton
{
    public string Id => "tail-weather-dynamic";

    const string DefaultId = "TemperateWeather";

    string? currId;
    Texture2D? currTexture;

    readonly HashSet<DynamicTailDecalApplier> comps = [];

    public void Load()
    {
        eb.Register(this);
    }

    public Texture2D GetTexture(DynamicTailDecalApplier _)
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
        var texture = assets.LoadSafe<Texture2D>($"DynamicDecals/Weathers/Tail-{id}");

        if (texture is null)
        {
            return id == DefaultId
                ? throw new InvalidOperationException($"Default weather tail texture with id '{DefaultId}' is missing.")
                : LoadOrDefault(DefaultId);
        }

        return texture;
    }

    public void Register(DynamicTailDecalApplier comp) => comps.Add(comp);
    public void Unregister(DynamicTailDecalApplier comp) => comps.Remove(comp);

    [OnEvent]
    public void OnHazWeather(HazardousWeatherStartedEvent _) => OnNewWeather();

    [OnEvent]
    public void OnNewWeather(CycleStartedEvent _) => OnNewWeather();

    void OnNewWeather()
    {
        GetTexture(null!);

        foreach (var comp in comps)
        {
            comp.ShowTexture();
        }
    }

}
