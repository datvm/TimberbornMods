global using TransparentTerrain.Services;
global using TransparentTerrain.UI;

namespace TransparentTerrain;

[Context("Game")]
public class MGameConfig : Configurator
{
    public override void Configure()
    {
        this
            .BindSingleton<TransparentTerrainService>()

            .BindTransient<TransparentTerrainDialog>()
            .BindSingleton<TransparentTerrainButton>();
        ;
    }
}
