namespace ConfigurableIrrigation;

[Context("MainMenu")]
[Context("Game")]
public class SettingsConfig : Configurator
{
    public override void Configure()
    {
        Bind<MSettings>().AsSingleton();
    }
}

public class ModStater : IModStarter
{

    public void StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(ConfigurableIrrigation)).PatchAll();
    }

}
