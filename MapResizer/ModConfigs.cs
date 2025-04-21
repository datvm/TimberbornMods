global using MapResizer.UI;

namespace MapResizer;

[Context("Game")]
public class ModGameConfig : Configurator
{
    public override void Configure()
    {
        Bind<MapResizerDialogTrigger>().AsSingleton();
        Bind<MapResizerService>().AsSingleton();
    }
}

public class ModStarter : IModStarter
{

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(MapResizer)).PatchAll();
    }

}