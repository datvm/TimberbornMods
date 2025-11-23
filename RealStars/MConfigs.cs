global using RealStars.Services;

namespace RealStars;

[Context("Game")]
public class MGameConfig : Configurator
{

    public override void Configure()
    {
        Bind<RealStarMaterialService>().AsSingleton();
    }

}
