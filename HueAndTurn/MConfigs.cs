global using HueAndTurn.Components;
global using HueAndTurn.Services;
global using HueAndTurn.UI;

namespace HueAndTurn;

[Context("MainMenu")]
public class ModMainMenuConfig : Configurator
{
    public override void Configure()
    {
    }
}

[Context("Game")]
[Context("MapEditor")]
public class ModGameConfig : Configurator
{
    public override void Configure()
    {
        this
            .BindSingleton<ColorHighlighter>()
            .BindSingleton<HueAndTurnMassApplier>()
            .BindSingleton<TransparencyShaderService>()

            .BindFragment<HueAndTurnFragment>()
            .BindTemplateModule(h => h
                .AddDecorator<BlockObjectSpec, HueAndTurnComponent>()
            )
        ;

    }
}
