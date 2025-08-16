global using WirelessCoil.Components;
global using WirelessCoil.Services;
global using WirelessCoil.UI;

namespace WirelessCoil;

[Context("Game")]
public class ModGameConfig : Configurator
{
    public override void Configure()
    {
        this
            .BindSingleton<WirelessCoilService>()

            .BindFragment<WirelessCoilFragment>()

            .BindTemplateModule(h => h
                .AddDecorator<WirelessCoilSpec, WirelessCoilComponent>()

                .AddDecorator<WirelessCoilSpec, BuildingLighting>()
                .AddDecorator<WirelessCoilSpec, PoweredNetworkLightController>()                
            );

    }
}
