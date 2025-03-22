namespace VerticalFarming;

[Context("MainMenu")]
public class MenuModConfig : Configurator
{
    public override void Configure()
    {
        Bind<MSettings>().AsSingleton();
    }
}

[Context("Game")]
public class GameModConfig : Configurator
{
    public override void Configure()
    {
        Bind<MSettings>().AsSingleton();
        Bind<ModCursorService>().AsSingleton();
    }
}

public class ModStarter : IModStarter
{
    public static readonly bool HasMoreMod = AppDomain.CurrentDomain.GetAssemblies().Any(q => q.GetName().Name == "MoreModLogs");

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(VerticalFarming)).PatchAll();
    }

    public static void Log(Func<string> message)
    {
        if (!HasMoreMod) { return; }

        Debug.Log($"{nameof(VerticalFarming)}: " + message());
    }

}