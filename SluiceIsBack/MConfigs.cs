global using SluiceIsBack.Components;
global using SluiceIsBack.UI;
global using Sluice = SluiceIsBack.Components.Sluice;
global using SluiceState = SluiceIsBack.Components.SluiceState;
global using SluiceSynchronizer = SluiceIsBack.Components.SluiceSynchronizer;
global using SluiceFragment = SluiceIsBack.UI.SluiceFragment;
global using SluiceMarker = SluiceIsBack.UI.SluiceMarker;
global using SluiceToggleFactory = SluiceIsBack.UI.SluiceToggleFactory;

namespace SluiceIsBack;

[Context("Game")]
public class MGameConfig : Configurator
{

    public override void Configure()
    {
        Bind<Sluice>().AsTransient();
        Bind<SluiceState>().AsTransient();
        Bind<SluiceSynchronizer>().AsTransient();

        Bind<SluiceMarker>().AsTransient();
        Bind<SluiceFragment>().AsSingleton();
        Bind<SluiceToggleFactory>().AsSingleton();

        MultiBind<TemplateModule>().ToProvider(() =>
        {
            var b = new TemplateModule.Builder();
            
            b.AddDecorator<LVSluiceSpec, Sluice>();
            b.AddDecorator<Sluice, SluiceState>();
            b.AddDecorator<Sluice, WaterObstacleController>();

            b.AddDecorator<Sluice, SluiceMarker>();
            b.AddDecorator<Sluice, WaterDirectionPreviewMarker>();

            return b.Build();
        }).AsSingleton();

        MultiBind<EntityPanelModule>().ToProvider<EntityPanelConfig>().AsSingleton();
    }

    public class EntityPanelConfig(SluiceFragment sluiceFragment) : IProvider<EntityPanelModule>
    {
        public EntityPanelModule Get()
        {
            var builder = new EntityPanelModule.Builder();
            builder.AddTopFragment(sluiceFragment);
            return builder.Build();
        }
    }

}
