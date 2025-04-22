global using TImprove4Mods.Services;

namespace TImprove4Mods;


[Context("MainMenu")]
public class ModMenuConfig : Configurator
{

    public override void Configure()
    {
        Bind<FileDialogService>().AsSingleton();
        Bind<LegacyModWarningService>().AsSingleton();
        Bind<ModManagerBoxService>().AsSingleton();
        Bind<ModManagementService>().AsSingleton();
    }

}

public class ModStarter : IModStarter
{
    public static bool HasLegacyMod;

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        HasLegacyMod = AppDomain.CurrentDomain.GetAssemblies()
            .Any(q => q.GetName().Name == "ToggleAllMods");
    }

}