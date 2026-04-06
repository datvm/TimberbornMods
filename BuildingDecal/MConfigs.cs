global using BuildingDecal.Components;
global using BuildingDecal.Services;
global using BuildingDecal.Specs;
global using BuildingDecal.UI;
global using ModdableDecalGroups.Services;

namespace BuildingDecal;

[Context("Game")]
public class ModGameConfig : Configurator
{
    public override void Configure()
    {
        this
            .BindSingleton<BuildingDecalProvider>()
            .BindSingleton<BuildingDecalPositionService>()
            .BindSingleton<BuildingDecalClipboard>()

            .BindFragment<BuildingDecalFragment>()
            .BindSingleton<BuildingDecalSelectDialog>()

            .BindTemplateModule(h => h
                .AddDecorator<BuildingSpec, BuildingDecalComponent>()
            );
    }
}
