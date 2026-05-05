namespace DynamicTailsBanners.Services.Implementations;

[MultiBind(typeof(IDynamicDecalProvider))]
public class WeatherDynamicTailProvider(
    IAssetLoader assets,
    WeatherIdService weatherIdService,
    EventBus eb
) : WeatherProviderBase<DynamicTailDecal>(assets, weatherIdService, eb), IDynamicTailDecalProvider
{

    public override string Id => "tail-weather-dynamic";

    protected override string GetWeatherPath(string id) => $"DynamicDecals/Weathers/Tail-{id}";

}
