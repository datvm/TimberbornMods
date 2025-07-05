global using TimberDump.Services;

namespace TimberDump;

[Context("MainMenu")]
public class ModMenuConfig : Configurator
{
    static readonly MethodInfo MultibindMethod = typeof(IBindingBuilder<IDumper>)
        .GetMethod(nameof(IBindingBuilder<>.To));

    public override void Configure()
    {
        // These are because they are not available in menu
        // From SpriteOperationsConfigurator
        Bind<SpriteResizer>().AsSingleton();
        Bind<SpriteFlipper>().AsSingleton();
        MultiBind<IDeserializer>().To<UISpriteDeserializer>().AsSingleton();
        MultiBind<IDeserializer>().To<FlippedSpriteDeserializer>().AsSingleton();

        Bind<DumpController>().AsSingleton();
        Bind<DumpService>().AsSingleton();

        var dumpers = typeof(ModMenuConfig).Assembly.GetTypes()
            .Where(q => q.IsClass && !q.IsAbstract && q.GetInterfaces().Contains(typeof(IDumper)));

        foreach (var dumper in dumpers)
        {
            var binder = MultiBind<IDumper>();

            var bindMethod = MultibindMethod.MakeGenericMethod([dumper])
                ?? throw new Exception("Not found");

            var bound = (IScopeAssignee)bindMethod.Invoke(binder, []);
            bound.AsSingleton();
        }
    }
}
