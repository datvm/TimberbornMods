global using TImprove4Mods.Services;
global using TImprove4Mods.Models;

namespace TImprove4Mods;


[Context("MainMenu")]
public class ModMenuConfig : Configurator
{

    public override void Configure()
    {
        this.TryBindingSystemFileDialogService();

        this
            .BindSingleton<ModCompWarningService>()
            .BindSingleton<ModManagerBoxService>()
            .BindSingleton<ModManagementService>()
            .BindSingleton<ModSettingBoxService>();
    }

}
public class ModStarter : IModStarter
{

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        ModCompatibilityService.Instance.Init(modEnvironment.ModPath);
    }

}