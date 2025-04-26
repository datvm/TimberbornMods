namespace MapResizer;

public class ModCommonConfig : Configurator
{
    public override void Configure()
    {
        Bind<MapResizerController>().AsSingleton();
        Bind<MapResizeService>().AsSingleton();
        Bind<TerrainMapResizeService>().AsSingleton();
        Bind<ColumnTerrainMapResizeService>().AsSingleton();
        Bind<SoilMapResizeService>().AsSingleton();
        Bind<WaterMapResizeService>().AsSingleton();
        Bind<BlockObjectResizeValidationService>().AsSingleton();
        Bind<PlantingMapResizer>().AsSingleton();
    }
}

[Context("Game")]
public class ModGameConfig : ModCommonConfig
{
    public override void Configure()
    {
        base.Configure();

        Bind<ISaverService>().To<GameSaverService>().AsSingleton();
    }

}

[Context("MapEditor")]
public class ModMapEditorConfig : ModCommonConfig
{
    public override void Configure()
    {
        base.Configure();

        Bind<ISaverService>().To<MapEditorSaverService>().AsSingleton();
    }
}

public class ModStarter : IModStarter
{

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(MapResizer)).PatchAll();
    }

}