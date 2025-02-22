namespace BuffDebuff;

public class ModConfig : Configurator
{

    public override void Configure()
    {
        Bind<IBuffEntityService>().To<BuffEntityService>().AsSingleton();
        Bind<IBuffableService>().To<BuffableService>().AsSingleton();
        Bind<IBuffService>().To<BuffService>().AsSingleton();

        Bind<TemplateModule>().ToProvider(() =>
        {
            TemplateModule.Builder builder = new();

            builder.AddDecorator<EntityComponent, BuffableComponent>();

            return builder.Build();
        }).AsSingleton();
    }

}
