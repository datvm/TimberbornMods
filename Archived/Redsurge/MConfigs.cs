global using Redsurge.Weathers;
global using Redsurge.Components;

namespace Redsurge;

[Context("MainMenu")]
public class ModMainMenuConfig : Configurator
{
    public override void Configure()
    {
        this
            .BindHazardousWeather<RedsurgeWeather, RedsurgeWeatherSettings>(true)
        ;
    }
}

[Context("Game")]
public class ModGameConfig : Configurator
{
    public override void Configure()
    {
        this
            .BindHazardousWeather<RedsurgeWeather, RedsurgeWeatherSettings>(false)

            .BindTemplateModule(h => h
                .AddDecorator<WaterSource, RedsurgeWaterStrengthModifier>()
                .AddDecorator<WaterSourceContamination, RedsurgeWaterContaminationController>()
            )
        ;
    }
}
