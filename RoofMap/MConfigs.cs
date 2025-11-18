namespace RoofMap;

[Context("Game")]
[Context("MapEditor")]
public class MConfigs : Configurator
{
    public override void Configure()
    {
        Bind<RoofMapService>().AsSingleton();
    }
}
