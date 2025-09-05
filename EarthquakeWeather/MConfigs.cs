global using EarthquakeWeather.Components;
global using EarthquakeWeather.Weathers;
global using EarthquakeWeather.Renovations;
global using EarthquakeWeather.Helpers;
global using EarthquakeWeather.Services;

namespace EarthquakeWeather;

[Context("MainMenu")]
public class ModMainMenuConfig : Configurator
{
    public override void Configure()
    {
        this
            .TryBindingCameraShake(true)
            .BindHazardousWeather<Earthquake, EarthquakeWeatherSettings>(true)
        ;
    }
}

[Context("Game")]
public class ModGameConfig : Configurator
{
    public override void Configure()
    {
        this
            .TryBindingCameraShake(false)

            .BindSingleton<EarthquakeRegistry>()
            .BindSingleton<EarthquakeAreaService>()
            .BindSingleton<EarthquakeStrikeService>()
            .BindSingleton<EarthquakeConstructionDamageService>()
            .BindSingleton<EarthquakeNotificationService>()
            .BindSingleton<EarthquakeInjuryService>()

            .MultiBindSingleton<IRenovationProvider, EqImmunityProvider>()
            .MultiBindSingleton<IRenovationProvider, EqDurabilityProvider>()
            .MultiBindSingleton<IRenovationProvider, EqDamageReductionProvider>()
            .MultiBindSingleton<IRenovationProvider, EqPileReinforcedProvider>()

            .MultiBindSingleton<IDevModule, EarthquakeDevModule>()

            .BindHazardousWeather<Earthquake, EarthquakeWeatherSettings>(false)

            .BindTemplateModule(h => h
                .AddDecorator<BuildingHPComponentSpec, EarthquakeComponent>()
                .AddDecorator<EarthquakeComponent, EarthquakeReinforcementComponent>()
                .AddDecorator<EarthquakeComponent, EarthquakeBuildingBlocker>()
                .AddDecorator<StockpileSpec, EarthquakeStockpileComponent>()
            )
        ;
    }
}
