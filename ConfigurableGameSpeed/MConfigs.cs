namespace ConfigurableGameSpeed;

[Context("MainMenu")]
public class ModMainMenuConfig : Configurator
{
    public override void Configure()
    {
        Bind<MSettings>().AsSingleton();
    }
}

[Context("Game")]
[Context("MapEditor")]
public class ModMapConfig : Configurator
{
    public override void Configure()
    {
        Bind<MSettings>().AsSingleton();
    }
}

public class ModStarter : IModStarter
{
    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(ConfigurableGameSpeed)).PatchAll();
    }
}
