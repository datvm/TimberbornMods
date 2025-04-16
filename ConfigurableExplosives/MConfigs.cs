namespace ConfigurableExplosives;

[Context("MainMenu")]
public class ModMenuConfig : Configurator
{
    
    public override void Configure()
    {
        Bind<MSettings>().AsSingleton();
    }

}

[Context("Game")]
public class ModGameConfig : Configurator
{

    public override void Configure()
    {
        Bind<MSettings>().AsSingleton();
        Bind<ConfigurableDynamiteCopyTool>().AsSingleton();
        this.BindFragments<EntityPanelFragmentProvider<ConfigurableDynamiteFragment>>();

        MultiBind<TemplateModule>().ToProvider(() =>
        {
            TemplateModule.Builder b = new();
            b.AddDecorator<Dynamite, ConfigurableDynamiteComponent>();
            return b.Build();
        }).AsSingleton();
    }

}

public class ModStarter : IModStarter
{

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(ConfigurableExplosives)).PatchAll();
    }

}