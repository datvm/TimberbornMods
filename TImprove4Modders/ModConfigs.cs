namespace TImprove4Modders;

[Context("MainMenu")]
[Context("Game")]
[Context("MapEditor")]
public class AllContextConfig : Configurator
{
    public override void Configure()
    {
        Bind<MSettings>().AsSingleton();
        Bind<QuickQuitService>().AsSingleton();
    }

}

[Context("Game")]
[Context("MapEditor")]
public class NonMenuContextConfig : Configurator
{
    public override void Configure()
    {
        Bind<AutoDevModeService>().AsSingleton();
    }

}

public class ModStarter : IModStarter
{

    public void StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(TImprove4Modders)).PatchAll();
    }

}