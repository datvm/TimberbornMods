namespace TImprove.Services;

[Context("Game")]
public class GameModConfigurator : IConfigurator
{
    public void Configure(IContainerDefinition containerDefinition)
    {
        containerDefinition.Bind<TImproveGameService>().AsSingleton();
    }
}

[Context("MainMenu")]
[Context("Game")]
[Context("MapEditor")]
public class AllContextModConfigurator : IConfigurator
{
    public void Configure(IContainerDefinition containerDefinition)
    {
        containerDefinition.Bind<ModSettings>().AsSingleton();
        containerDefinition.Bind<QuickQuitService>().AsSingleton();
    }
}