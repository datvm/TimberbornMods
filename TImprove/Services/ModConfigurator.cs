
namespace TImprove.Services;

[Context("Game")]
public class GameModConfigurator : IConfigurator
{
    public void Configure(IContainerDefinition containerDefinition)
    {
        containerDefinition.Bind<TImproveGameService>().AsSingleton();
        containerDefinition.Bind<GameDepServices>().AsSingleton();
    }
}

[Context("MainMenu")]
[Context("Game")]
[Context("MapEditor")]
public class AllContextModConfigurator : IConfigurator
{
    public void Configure(IContainerDefinition containerDefinition)
    {
        containerDefinition.Bind<MSettings>().AsSingleton();
        containerDefinition.Bind<QuickQuitService>().AsSingleton();
    }
}

[Context("Game")]
[Context("MapEditor")]
public class CameraModConfigurator : IConfigurator
{
    public void Configure(IContainerDefinition containerDefinition)
    {
#if TIMBER6
        containerDefinition.Bind<CoordsPanel>().AsSingleton();
        containerDefinition.Bind<CoordsService>().AsSingleton();
#endif
    }
}