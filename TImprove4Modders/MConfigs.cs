namespace TImprove4Modders;

public class CommonConfig : Configurator
{
    public override void Configure()
    {
        this
            .TryBindingSystemFileDialogService()

            .BindSingleton<MSettings>()

            .BindSingleton<QuickQuitService>()
            .BindSingleton<DevModeService>()
        ;
    }
}

public class NonMenuConfig : CommonConfig
{

    public override void Configure()
    {
        base.Configure();

        this
            .MultiBindSingleton<IDevModule, PrintUiModule>()
            .MultiBindSingleton<IDevModule, ScienceModule>()
            .MultiBindSingleton<IDevModule, PlantModule>()
            .MultiBindSingleton<IDevModule, ComponentModule>()
            .MultiBindSingleton<IDevModule, BuildingsModule>()
        ;
    }

}

[Context("MainMenu")]
public class ModMenuConfig : CommonConfig
{
    public override void Configure()
    {
        base.Configure();

        this
            .BindSingleton<ModManagementService>()
            .BindSingleton<SaveModsDialogService>()
        ;
    }
}

[Context("Game")]
public class ModGameConfig : NonMenuConfig
{
    public override void Configure()
    {
        base.Configure();

        this
            .MultiBindSingleton<IDevModule, TimeModule>()
        ;
    }
}

[Context("MapEditor")]
public class ModMapEditorConfig : NonMenuConfig
{
    public override void Configure()
    {
        base.Configure();
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