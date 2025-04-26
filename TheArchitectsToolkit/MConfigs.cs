namespace TheArchitectsToolkit;

[Context("MainMenu")]
[Context("Game")]
[Context("MapEditor")]
public class AllContextModConfigurator : IConfigurator
{
    public void Configure(IContainerDefinition containerDefinition)
    {
        containerDefinition.Bind<MSettings>().AsSingleton();
        containerDefinition.Bind<ToolkitService>().AsSingleton();
    }
}

[Context("MapEditor")]
public class MapEditorModConfigurator : Configurator
{
    public override void Configure()
    {
        Bind<CopierController>().AsSingleton();
        Bind<BlockObjectCopierService>().AsSingleton();
    }
}

public class ModStarter : IModStarter
{

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        var harmony = new Harmony(nameof(TheArchitectsToolkit));
        harmony.PatchAll();
    }

}