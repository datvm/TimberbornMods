namespace DynamicTailsBanners.Services.Implementations;

[MultiBind(typeof(IDynamicDecalProvider))]
public class WeatherDynamicBannerProvider(
    IAssetLoader assets,
    WeatherIdService weatherIdService,
    EventBus eb
) : WeatherProviderBase<DynamicBuildingDecal>(assets, weatherIdService, eb), IDynamicBannerDecalProvider
{

    public override string Id => "banner-weather-dynamic";

    protected override string GetWeatherPath(string id) => $"DynamicDecals/Weathers/Banner-{id}";

}
