global using TransparentTerrain.Services;
global using TransparentTerrain.UI;

namespace TransparentTerrain;

[Context("Game")]
[Context("MapEditor")]
public class MGameConfig : Configurator
{
    public override void Configure()
    {
        this
            .BindSingleton<TransparentTerrainService>()
            .BindSingleton<TransparentTerrainTopMeshService>()

            .BindTransient<TransparentTerrainDialog>()
            .BindSingleton<TransparentTerrainButton>();
        ;
    }
}
