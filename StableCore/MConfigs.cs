global using StableCore.Components;
global using StableCore.UI;

namespace StableCore;

[Context("Game")]
public class GameConfig : Configurator
{
    public override void Configure()
    {
        this
            .BindFragment<StableCoreFragment>()

            .BindTemplateModule(h => h
                .AddDecorator<StablizedCoreSpec, StablizedCoreComponent>()
            )
        ;
    }
}
