
namespace BuildingHP;

public class CommonConfig : Configurator
{
    public override void Configure()
    {
        this
            .BindSingleton<BuildingMaterialDurabilityService>()
            .BindSingleton<MSettings>()
        ;
    }
}

[Context("MainMenu")]
public class ModMainMenuConfig : CommonConfig{}

[Context("Game")]
public class ModGameConfig : CommonConfig
{
    public override void Configure()
    {
        this
            .BindTemplateModule(h => h
                .AddDecorator<BuildingSpec, BuildingHPComponentSpec>()
                .AddDecorator<BuildingHPComponentSpec, BuildingHPComponent>()
                .AddDecorator<BuildingHPComponentSpec, BuildingMaterialDurabilityComponent>()
            )
        ;
    }
}
