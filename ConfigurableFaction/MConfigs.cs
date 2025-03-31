namespace ConfigurableFaction;

[Context("MainMenu")]
public class MenuModConfig : Configurator
{
    public override void Configure()
    {
        Bind<MSettings>().AsSingleton();
        Bind<FactionBuildingService>().AsSingleton();
    }
}

[Context("Game")]
public class GameModConfig : Configurator
{
    public override void Configure()
    {
        Bind<MSettings>().AsSingleton();
        Bind<FactionBuildingService>().AsSingleton();
        Bind<BuildingDumpService>().AsSingleton();
    }
}

public class ModStarter : IModStarter
{

    public static string ModFolder
    {
        get;
        private set
        {
            FactionFolder = Path.Combine(value, "Factions");
            Directory.CreateDirectory(FactionFolder);
        }
    } = null!;
    public static string FactionFolder { get; private set; } = null!;

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        ModFolder = modEnvironment.ModPath;

        new Harmony(nameof(ConfigurableFaction)).PatchAll();
    }
}
