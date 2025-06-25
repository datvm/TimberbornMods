namespace UselessMachine;

public class MainMenuService(ModRepository mods) : ILoadableSingleton
{
    public void Load()
    {
        ModPlayerPrefsHelper.ToggleMod(false, mods.EnabledMods.First(q => q.Manifest.Id == nameof(UselessMachine)));
        GameQuitter.Quit();
    }
}

[Context("MainMenu")]
public class ModMenuConfig : Configurator
{
    public override void Configure()
    {
        Bind<MainMenuService>().AsSingleton();
    }
}