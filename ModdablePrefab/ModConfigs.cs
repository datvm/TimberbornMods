namespace ModdablePrefab;

[Context("MainMenu")]
public class MainMenuModConfig : Configurator
{

    public override void Configure()
    {
        MultiBind<IPrefabModder>().To<SpecPrefabModder>().AsSingleton();
        Bind<PrefabModderRegistry>().AsSingleton();
    }

}

public class ModStarter : IModStarter
{
    
    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(ModdablePrefab)).PatchAll();
    }

}
