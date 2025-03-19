global using Timberborn.Buildings;

namespace ColorfulBuildings;

[Context("Game")]
public class GameModConfig : Configurator
{
    public class FragmentsProvider(BuildingColorFragment buildingColor) : EntityPanelFragmentProvider([buildingColor]);

    public override void Configure()
    {
        MultiBind<TemplateModule>().ToProvider(() =>
        {
            TemplateModule.Builder b = new();
            b.AddDecorator<BuildingSpec, BuildingColorComponent>();
            return b.Build();
        }).AsSingleton();

        this.BindFragments<FragmentsProvider>();
    }

}