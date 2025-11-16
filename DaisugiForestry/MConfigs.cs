global using DaisugiForestry.Services;
global using DaisugiForestry.Components;

namespace DaisugiForestry;

[Context("Game")]
[Context("MapEditor")]
public class ModGameConfig : Configurator
{
    public override void Configure()
    {
        Bind<TreeYieldMaterialService>().AsSingleton();
    }

}
