global using MapResizer.UI;

namespace MapResizer;

[Context("Game")]
public class ModGameConfig : Configurator
{
    public override void Configure()
    {
        Bind<MapResizerDialogTrigger>().AsSingleton();

        Bind<MapResizeService>().AsSingleton();
        Bind<TerrainMapResizeService>().AsSingleton();
        Bind<ColumnTerrainMapResizeService>().AsSingleton();
        Bind<SoilMapResizeService>().AsSingleton();
        Bind<WaterMapResizeService>().AsSingleton();
    }
}

public class ModStarter : IModStarter
{

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(MapResizer)).PatchAll();
    }

}