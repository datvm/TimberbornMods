global using BathBombs.Components;
global using BathBombs.Services;
global using BathBombs.UI;

namespace BathBombs;

[Context("Game")]
public class MGameConfig : Configurator
{

    public override void Configure()
    {
        Bind<BathBombService>().AsSingleton();
        Bind<BathBombFragment>().AsSingleton();

        Bind<BathBombComponent>().AsTransient();
        Bind<BathBombIndicator>().AsTransient();

        MultiBind<TemplateModule>().ToProvider(() =>
        {
            var b = new TemplateModule.Builder();
            b.AddDecorator<BathBombSpec, BathBombComponent>();
            b.AddDecorator<BathBombComponent, BathBombIndicator>();
            return b.Build();
        }).AsSingleton();

        MultiBind<EntityPanelModule>().ToProvider<FragmentProvider>().AsSingleton();
    }

    class FragmentProvider(BathBombFragment f) : IProvider<EntityPanelModule>
    {
        public EntityPanelModule Get()
        {
            var b = new EntityPanelModule.Builder();
            b.AddTopFragment(f);
            return b.Build();
        }
    }

}
