namespace ModdablePrefab;

[Context("MainMenu")]
public class MainMenuModConfig : Configurator
{

    public override void Configure()
    {
        Bind<SpecPrefabModder>().AsSingleton();
    }

}

public class ModStarter : IModStarter
{

    public static readonly bool HasMoreMod = AppDomain.CurrentDomain.GetAssemblies().Any(q => q.GetName().Name == "MoreModLogs");

    public static void Log(Func<string> message)
    {
        if (!HasMoreMod) { return; }

        Debug.Log($"{nameof(ModdablePrefab)}: " + message());
    }

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(ModdablePrefab)).PatchAll();
    }



}
