global using HueAndTurn.Components;
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
        Bind<ColorHighlighter>().AsSingleton();
        Bind<HueAndTurnMassApplier>().AsSingleton();
        this.BindFragments<EntityPanelFragmentProvider<HueAndTurnFragment>>();

        MultiBind<TemplateModule>().ToProvider(() =>
        {
            TemplateModule.Builder b = new();
            b.AddDecorator<BlockObjectSpec, HueAndTurnComponent>();
            return b.Build();
        }).AsSingleton();
    }
}
