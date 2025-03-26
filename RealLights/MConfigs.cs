global using RealLights.Components;
global using RealLights.Managements;
global using Timberborn.Buildings;
global using RealLights.Graphical;

namespace RealLights;


[Context("Game")]
public class GameModConfig : Configurator
{
    public class FragmentsProvider(RealLightsFragment light) : EntityPanelFragmentProvider([light]);

    public override void Configure()
    {
        Bind<RealLightsManager>().AsSingleton();
        this.BindFragments<FragmentsProvider>();

        MultiBind<TemplateModule>().ToProvider(() =>
        {
            TemplateModule.Builder b = new();
            b.AddDecorator<BuildingSpec, RealLightsComponent>();
            return b.Build();
        }).AsSingleton();
    }

}