global using BuildingDecal.Components;
global using BuildingDecal.Services;
global using BuildingDecal.Specs;
global using BuildingDecal.UI;
global using ModdableDecalGroups.Services;
global using ModdableTimberborn.BuildingSettings;

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

            .MultiBindSingleton<IBuildingSettings, BuildingDecalBuildingSettings>()

            .BindTemplateModule(h => h
                .AddDecorator<BuildingSpec, BuildingDecalComponent>()
            );
    }
}
