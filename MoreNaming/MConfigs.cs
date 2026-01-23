global using MoreNaming.Components;

namespace MoreNaming;

[Context("Game")]
public class ModGameConfig : Configurator
{

    public override void Configure()
    {
        Bind<EntityNamingComponent>().AsTransient();

        MultiBind<TemplateModule>().ToProvider(() =>
        {
            TemplateModule.Builder b = new();
            b.AddDecorator<LabeledEntitySpec, EntityNamingComponent>();
            return b.Build();
        }).AsSingleton();
    }

}
