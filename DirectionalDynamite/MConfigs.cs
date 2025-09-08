global using DirectionalDynamite.Components;
global using DirectionalDynamite.UI;
global using DirectionalDynamite.Services;

namespace DirectionalDynamite;

[Context("Game")]
public class ModGameConfig : Configurator
{
    public override void Configure()
    {
        this
            .BindSingleton<DirectionalDynamiteService>()
            .BindSingleton<TerrainDestroyService>()

            .BindFragment<DirectionalDynamiteFragment>()

            .BindTemplateModule(h => h
                .AddDecorator<Dynamite, DirectionalDynamiteComponent>()
            )
        ;
    }
}
