global using BlueprintRelics.Components;
global using BlueprintRelics.Services;
global using BlueprintRelics.Specs;
global using BlueprintRelics.UI;
global using BlueprintRelics.Services.Rewards;

global using Random = UnityEngine.Random;
global using ProgressBar = Timberborn.CoreUI.ProgressBar;

namespace BlueprintRelics;

[Context("Game")]
public class MGameConfig : Configurator
{
    public override void Configure()
    {
        this
            .BindSingleton<BlueprintRelicsRegistry>()
            .BindSingleton<BlueprintRelicsSpawner>()
            .BindSingleton<BlueprintRelicCollectorService>()

            .MultiBindSingleton<IDevModule, BlueprintRelicsDevModule>()

            .BindFragment<BlueprintRelicFragment>()
            .BindTransient<RelicRewardDialog>()

            .BindTemplateModule(h => h
                .AddDecorator<BlueprintRelicSpec, BlueprintRelicComponent>()
                .AddDecorator<BlueprintRelicComponent, BlueprintRelicCollector>()
            )
        ;
    }
}
