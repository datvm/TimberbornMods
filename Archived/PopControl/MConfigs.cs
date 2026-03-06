global using PopControl.Components;
global using PopControl.Services;
global using PopControl.UI;

namespace PopControl;

[Context("Game")]
public class ModGameConfig : Configurator
{
    public override void Configure()
    {
        this
            .BindSingleton<PopControlRegistry>()
            .BindSingleton<PopControlService>()

            .BindSingleton<PopControlDialog>()

            .BindFragment<PopControlFragment>()

            .BindTemplateModule(h => h
                .AddDecorator<BreedingPodSpec, BreedingPodPopControlBlocker>()
                .AddDecorator<BotManufactorySpec, BotManufactoryPopControlBlocker>()
            )
        ;
    }
}
