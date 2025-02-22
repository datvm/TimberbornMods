

namespace BuffDebuffDemo;

[Context("Game")]
public class GameModConfig : Configurator
{

    public override void Configure()
    {
        Bind<TemplateModule>().ToProvider(TemplateModuleProvider).AsSingleton();
    }

    static TemplateModule TemplateModuleProvider()
    {
        TemplateModule.Builder builder = new();
        builder.AddDecorator<EntityComponent, BuffableComponent>();

        return builder.Build();
    }

}

public class ModStarter : IModStarter
{

    public void StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(BuffDebuffDemo)).PatchAll();
    }

}