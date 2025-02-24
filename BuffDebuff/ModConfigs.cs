
namespace BuffDebuff;

[Context("Game")]
public class ModConfig : Configurator
{

    public class FragmentProvider(BuffPanel buff, DebuffPanel debuff) : EntityPanelFragmentProvider([buff, debuff]);

    public override void Configure()
    {
        Bind<IBuffEntityService>().To<BuffEntityService>().AsSingleton();
        Bind<IBuffableService>().To<BuffableService>().AsSingleton();
        Bind<IBuffService>().To<BuffService>().AsSingleton();

        MultiBind<TemplateModule>().ToProvider(static () =>
        {
            TemplateModule.Builder builder = new();

            builder.AddDecorator<Transform, BuffableComponent>();

            return builder.Build();
        }).AsSingleton();

        this.BindFragments<FragmentProvider>();
    }

}
