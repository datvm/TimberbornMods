global using StableCore.Components;
global using StableCore.UI;

namespace StableCore;

[Context("Game")]
public class GameConfig : Configurator
{
    public override void Configure()
    {
        this
            .BindTransient<StablizedCoreComponent>()

            .BindFragment<StableCoreFragment>()

            .BindTemplateModule(h => h
                .AddDecorator<StablizedCoreSpec, StablizedCoreComponent>()
            )
        ;
    }
}
