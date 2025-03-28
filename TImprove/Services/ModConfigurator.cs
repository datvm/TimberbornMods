﻿
namespace TImprove.Services;

[Context("Game")]
public class GameModConfigurator : IConfigurator
{
    public void Configure(IContainerDefinition containerDefinition)
    {
        containerDefinition.Bind<TImproveGameService>().AsSingleton();
        containerDefinition.Bind<GameDepServices>().AsSingleton();
        containerDefinition.Bind<RealTimeComponent>().AsSingleton();
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
        containerDefinition.Bind<CoordsPanel>().AsSingleton();
        containerDefinition.Bind<CoordsService>().AsSingleton();
    }
}