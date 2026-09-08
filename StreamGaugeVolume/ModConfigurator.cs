global using StreamGaugeVolume.UI;
using Timberborn.TemplateSystem;

namespace StreamGaugeVolume;

[Context("Game")]
public class GameModConfigurator : IConfigurator
{
    public void Configure(IContainerDefinition containerDefinition)
    {
        containerDefinition.Bind<StreamGaugeVolumeService>().AsSingleton();

        containerDefinition.Bind<StreamGaugeVolumeFragmentBuilder>().AsTransient();
        containerDefinition.Bind<StreamGaugeVolumeFragment>().AsTransient();

        containerDefinition.MultiBind<EntityPanelModule>().ToProvider<EntityPanelModuleProvider>().AsSingleton();
        containerDefinition.MultiBind<TemplateModule>().ToProvider(ProvideTemplateModule).AsSingleton();
    }

    class EntityPanelModuleProvider(StreamGaugeVolumeFragment fragment) : IProvider<EntityPanelModule>
    {

        public EntityPanelModule Get()
        {
            EntityPanelModule.Builder builder = new();
            builder.AddMiddleFragment(fragment);
            return builder.Build();
        }
    }

    private static TemplateModule ProvideTemplateModule()
    {
        TemplateModule.Builder builder = new();
        builder.AddDecorator<StreamGauge, StreamGaugeVolumeComponent>();
        return builder.Build();
    }
}