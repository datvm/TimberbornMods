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
    
    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(ModdablePrefab)).PatchAll();
    }

}
