namespace TImprove4Modders;

public class CommonConfig : Configurator
{
    public override void Configure()
    {
        Bind<MSettings>().AsSingleton();
        Bind<QuickQuitService>().AsSingleton();
        Bind<DevModeService>().AsSingleton();
    }
}

public class NonMenuConfig : CommonConfig
{

    public override void Configure()
    {
        base.Configure();

        MultiBind<IDevModule>().To<PrintUiModule>().AsSingleton();
        MultiBind<IDevModule>().To<ScienceModule>().AsSingleton();
        MultiBind<IDevModule>().To<PlantModule>().AsSingleton();
        MultiBind<IDevModule>().To<ComponentModule>().AsSingleton();
    }

}

[Context("MainMenu")]
public class ModMenuConfig : CommonConfig
{
    public override void Configure()
    {
        base.Configure();

        Bind<ModManagementService>().AsSingleton();
        this.TryBindingSystemFileDialogService();
    }
}

[Context("Game")]
public class ModGameConfig : NonMenuConfig
{
    public override void Configure()
    {
        base.Configure();

        MultiBind<IDevModule>().To<TimeModule>().AsSingleton();
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