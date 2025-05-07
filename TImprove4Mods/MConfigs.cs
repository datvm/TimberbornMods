global using TImprove4Mods.Services;
global using TImprove4Mods.Models;

namespace TImprove4Mods;


[Context("MainMenu")]
public class ModMenuConfig : Configurator
{

    public override void Configure()
    {
        Bind<FileDialogService>().AsSingleton();
        Bind<ModCompWarningService>().AsSingleton();
        Bind<ModManagerBoxService>().AsSingleton();
        Bind<ModManagementService>().AsSingleton();
    }

}

public class ModStarter : IModStarter
{

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        ModCompatibilityService.Instance.Init(modEnvironment.ModPath);
    }

}