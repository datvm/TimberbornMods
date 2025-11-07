global using DaisugiForestry.Services;
global using DaisugiForestry.Components;

namespace DaisugiForestry;

[Context("Game")]
public class ModGameConfig : Configurator
{
    public override void Configure()
    {
        Bind<TreeYieldMaterialService>().AsSingleton();
    }

}
