namespace ConfigurableIrrigation;

[Context("MainMenu")]
[Context("Game")]
public class MModConfigs : Configurator
{
    public override void Configure()
    {
        Bind<MSettings>().AsSingleton();
    }
}

public class MStarter : IModStarter
{

    public void StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(ConfigurableIrrigation)).PatchAll();
    }

}
