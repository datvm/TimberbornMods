using RealLights.Services;

namespace RealLights;

[Context("Game")]
public class GameModConfig : GameAttributeConfigurator
{

    public override void Configure()
    {
        this
            .BindSingleton<RealLightsManager>()

            .BindFragment<RealLightsFragment>()

            .BindTemplateModule(h => h
                .AddDecorator<BuildingSpec, RealLightsComponent>()
            )
        ;
    }

}