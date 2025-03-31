using TImprove4Modders.DevModules;

namespace TImprove4Modders;

[Context("MainMenu")]
public class MenuContextConfig : Configurator
{
    public override void Configure()
    {
        Bind<ModManagementService>().AsSingleton();
    }
}

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
        Bind<DevModeService>().AsSingleton();

        MultiBind<IDevModule>().To<PrintUiModule>().AsSingleton();
        MultiBind<IDevModule>().To<ScienceModule>().AsSingleton();
        MultiBind<IDevModule>().To<PlantModule>().AsSingleton();
    }

}

public class ModStarter : IModStarter
{
    public static string ModPath { get; private set; } = null!;

    public void StartMod(IModEnvironment modEnvironment)
    {
        ModPath = modEnvironment.ModPath;

        new Harmony(nameof(TImprove4Modders)).PatchAll();
    }

}