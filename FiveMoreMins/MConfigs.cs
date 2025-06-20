namespace FiveMoreMins;

[Context("MainMenu")]
[Context("Game")]
public class ModConfig : Configurator
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
        new Harmony(nameof(FiveMoreMins)).PatchAll();
    }

}