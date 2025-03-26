global using RealLights.Components;
global using RealLights.Managements;
global using Timberborn.Buildings;
global using RealLights.Graphical;

namespace RealLights;

[Context("MainMenu")]
public class MenuModConfig : Configurator
{
    public override void Configure()
    {
        Bind<MSettings>().AsSingleton();
    }
}

[Context("Game")]
public class GameModConfig : Configurator
{
    public class FragmentsProvider(RealLightsFragment light) : EntityPanelFragmentProvider([light]);

    public override void Configure()
    {
        Bind<MSettings>().AsSingleton();

        Bind<RealLightsManager>().AsSingleton();
        this.BindFragments<FragmentsProvider>();

        MultiBind<TemplateModule>().ToProvider(() =>
        {
            TemplateModule.Builder b = new();
            b.AddDecorator<BuildingLighting, RealLightsComponent>();
            return b.Build();
        }).AsSingleton();
    }

}